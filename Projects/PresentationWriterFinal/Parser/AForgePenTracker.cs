using AForge.Imaging;
using AForge.Imaging.Filters;
using HSR.PresWriter.Containers;
using HSR.PresWriter.IO;
using HSR.PresWriter.IO.Events;
using HSR.PresWriter.PenTracking.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HSR.PresWriter.PenTracking
{
    /// <summary>
    /// Used to track pen points on camera output. AForge Filters are used to achieve that:
    /// 1.  Calculate the difference of two camera pictures in order
    ///     Optimization: Gaining difference pictures by searching first in a small area 
    ///     possibly containing the searched point.
    /// 2.  Grayscale and Threshold the difference picture by a defined Strategy
    /// 3.  Search for remaining blobs of the right size:
    ///     a) if one is found, it is a solution
    ///     b) if two are found, the intermediate is a solution
    ///     c) otherwise there is no solution (no pen found)
    /// 
    /// - private methods are synched, by a semaphore at the main frame processing method "onTrackPen".
    /// - public methods are synched seperately by csharp locking mechanism.
    /// </summary>
    public class AForgePenTracker : IPenTracker
    {
        private IPictureProvider _source;
        private FixedSizedQueue<VideoFrame> _frameBuffer;
        private FixedSizedQueue<PointFrame> _penPoints;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public FilterStrategy Strategy { get; set; }

        public AForgePenTracker(FilterStrategy strategy, IPictureProvider source)
        {
            _source = source;
            Strategy = strategy;
            _frameBuffer = new FixedSizedQueue<VideoFrame>(3);   // 3 video frames are used for discovering pen points
            _penPoints = new FixedSizedQueue<PointFrame>(10000); // 10000 points are kept
        }

        public void Start()
        {
            _source.FrameReady += onTrackPen;
        }

        public void Stop()
        {
            _source.FrameReady -= onTrackPen;
        }

        private void onTrackPen(object sender, FrameReadyEventArgs e)
        {
            /* We allow just 1 Thread to process a picture at a time.
             * If there is already a thread running, we discard the new 
             * frame after we waited for half the framerate. So we give 
             * time to catch up in two frames.
             */
            if (_semaphore.Wait(20))
            {
                var task = Task.Run(() =>
                {
                    try
                    {
                        // Time Keeper for Debugging
                        Stopwatch sw = new Stopwatch();
                        sw.Start();

                        processFrame(e.Frame);

                        long stage1 = sw.ElapsedMilliseconds;
                        sw.Stop();
                        //if (stage1 >= 40)
                        //{
                        Console.WriteLine("-> Pen Tracking Time: {0}", stage1);
                        //}

                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                });

                // rethrow Exception if necessary
                if (task.Exception != null)
                {
                    Debug.WriteLine("Pen Tracking Exception: " + task.Exception.ToString());
                }
            }
        }

        private void processFrame(VideoFrame currentFrame)
        {
            // We can only do our work if there is at least one frame in the buffer already
            if (this._frameBuffer.Count < 1)
            {
                // But we keep the frame for the next incoming processing request
                this._frameBuffer.Enqueue(currentFrame);
                return;
            }

            // We can do our work, since there is a previously taken frame.
            VideoFrame previousFrame = this._frameBuffer.Last(); 
            this._frameBuffer.Enqueue(currentFrame);

            // Status: 
            // - We now have references to a previous and a current video frame in the correct order.
            // - Additionally we have some previous points location (not necessarily matching to the previous video frame).
            try
            {
                // Reference to the last found point is needed to search more efficiently.
                // Efficiently means: Arround the last found point
                PointFrame previousPoint = this._penPoints.LastOrDefault();

                // At first we search for the pen near the old one (if there was one)
                IEnumerable<PenCandidate> candidates = null;
                if (previousPoint != null)
                {
                    candidates = this.findPenCandidatesInArea(
                         (Bitmap)previousFrame.Bitmap.Clone(),
                         (Bitmap)currentFrame.Bitmap.Clone(),
                         this.getEstimatedSearchArea(previousPoint)
                     );
                }

                // If there is no previous point (previousPoint == null) or we couldn't use the information 
                // of a previously tracked point (candidates == 0), we search the whole image.
                // WEAKNESS: if we detect one in our search area, but two are present on the whole picture, 
                // we give FALSE feedback. Therefore, the searcharea must be choosen carefully
                if (previousPoint == null || candidates.Count() == 0)
                {
                    candidates = this.findPenCandidatesInArea(
                        (Bitmap)previousFrame.Bitmap.Clone(), 
                        (Bitmap)currentFrame.Bitmap.Clone(), 
                        Rectangle.Empty
                    );
                }

                // Evaluate all the pen candidates for valid pen positions
                Point foundPoint = this.findPen(candidates);

                // TODO empty may be a valid point (0,0)
                if (foundPoint.IsEmpty)
                {
                    if (this.NoPenFound != null)
                    {
                        this.NoPenFound(this, null);
                    }
                    return;
                }

                // Put the qualified result in a new point frame and queue it to the result list
                PointFrame resultFrame = new PointFrame(currentFrame.Number, foundPoint, currentFrame.Timestamp);
                this._penPoints.Enqueue(resultFrame);

                if (this.PenFound != null)
                {
                    this.PenFound(this, new PenFoundEventArgs(resultFrame));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error in Frame Processing: \""+e.ToString()+"\"");
                // TODO Error Handling: Maybe we should catch everything for stability.
            }
        }

        /// <summary>
        /// At first we try to search near the last found point (in a rectangle of 100x100px)
        /// This means, that a point could have moved 50px in every direction. TODO: analyze moving direction and velocity
        /// </summary>
        /// <param name="lastFoundPoint"></param>
        /// <returns></returns>
        private Rectangle getEstimatedSearchArea(PointFrame lastFoundPoint)
        {
            int areaX = lastFoundPoint.Point.X - 50; // TODO Magic Number
            int areaY = lastFoundPoint.Point.Y - 50;
            return new Rectangle(areaX, areaY, 100, 100);
        }

        /// <summary>
        /// This is the time consuming part. We try to find pen candidates in two pictures 
        /// by their filtered difference in a specified area.
        /// </summary>
        /// <param name="previous">Previous video picture</param>
        /// <param name="current">Current video picture</param>
        /// <returns>Candidates</returns>
        private IEnumerable<PenCandidate> findPenCandidatesInArea(Bitmap previous, Bitmap current, Rectangle searchArea)
        {
            if (!searchArea.IsEmpty)
            {
                Crop cropFilter = new Crop(searchArea);
                previous = cropFilter.Apply(previous);
                current = cropFilter.Apply(current);
            }

            // calculate difference image
            this.Strategy.DifferenceFilter.OverlayImage = previous;
            Bitmap diffImage = current; //this.Strategy.DifferenceFilter.Apply(current);
            //diffImage.Save(@"c:\temp\images\diff-"+CurrentMillis.Millis+".png");

            // translate red parts to gray image
            Bitmap grayImage = this.Strategy.GrayFilter.Apply(diffImage);
            //a.Save(@"c:\temp\images\r2_grey16.bmp");

            // treshold the gray image
            Bitmap tresholdImage = this.Strategy.ThresholdFilter.Apply(grayImage);
            //tresholdImage.Save(@"c:\temp\images\blobs-" + CurrentMillis.Millis + ".png");

#if DEBUG
            if (DebugPicture != null)
            {
                DebugPicture(this, new DebugPictureEventArgs(new List<Bitmap>() { diffImage, grayImage, tresholdImage }));
            }
#endif

            // count white blobs (TODO ev. kann man den thresholdFilter wegschmeissen, siehe ctr parameter von BlobCounter)
            this.Strategy.BlobCounter.ProcessImage(tresholdImage);

            // frame found blobs and add information
            Rectangle[] rawCandidates = this.Strategy.BlobCounter.GetObjectsRectangles();
            List<PenCandidate> candidates = new List<PenCandidate>();
            foreach (Rectangle r in rawCandidates)
            {
                r.Offset(searchArea.Location); // Adjust Blob Location to search area
                candidates.Add(new PenCandidate(){
                    Rectangle = r,
                    WeightedCenter = PointTools.CalculateCenterPoint(r) // TODO more precise weightening
                });
            }

            return candidates;
        }

        /// <summary>
        /// Delivers a concrete interpretation of the algorithms resulting candidates.
        /// </summary>
        /// <param name="candidates">Zero or multiple found pen candidates</param>
        /// <returns>This is our final result: A pen position</returns>
        private Point findPen(IEnumerable<PenCandidate> candidates)
        {
            // Return intermediate point
            switch (candidates.Count())
            {
                case 0:
                    // No pen found or pen is not moving
                    // TODO (0,0) is actually a valid position!
                    return Point.Empty;
                case 1:
                    // Take center of the only found rectangle
                    return candidates.First().WeightedCenter;
                case 2:
                    PenCandidate candidateOne = candidates.ElementAt(0);
                    PenCandidate candidateTwo = candidates.ElementAt(1);

                    //PointFrame previousPointFrame = this.GetLastFrame();
                    //if (previousPointFrame == null)
                    //{
                    //    // If we don't have a previous point, then we can't say in which direction
                    //    // the pen is moving. We just interpolate.
                    //    return PointTools.CalculateCenterPoint(
                    //        candidateOne.WeightedCenter,
                    //        candidateTwo.WeightedCenter);
                    //}

                    //// We search the candidates for the previously recognized pen point
                    //// and return just the new one.
                    //if (candidateOne.Rectangle.Contains(previousPointFrame.Point))
                    //{
                    //    return candidateTwo.WeightedCenter;
                    //}
                    //if (candidateTwo.Rectangle.Contains(previousPointFrame.Point))
                    //{
                    //    return candidateOne.WeightedCenter;
                    //}

                    //// Both points are unknown!
                    //throw new NotImplementedException("TODO Invalid State. What to do?");

                    return PointTools.CalculateCenterPoint(
                        candidateOne.WeightedCenter,
                        candidateTwo.WeightedCenter);
                default:
                    throw new NotImplementedException("TODO Error Handling: more than two points are bad! Wrong Camera adjustment?");
            }
        }

        /// <summary>
        /// Get last processed frame
        /// </summary>
        public PointFrame GetLastFound()
        {
            lock (this._penPoints)
            {
                if (this._penPoints.Count > 0)
                {
                    return this._penPoints.Last();
                }
            }
            return null;
        }

        /// <summary>
        /// Estimates the pen position on a certain time.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public Point GetPenPoint(long timestamp)
        {
            PointFrame previousFrame = null;
            List<PointFrame>.Enumerator enumerator;

            lock (this._penPoints)
            {
                enumerator = this._penPoints.ToList().GetEnumerator();
                // Search a predecessor and a successor
                while (enumerator.MoveNext() && timestamp < enumerator.Current.Timestamp)
                {
                    if (timestamp == enumerator.Current.Timestamp)
                    {
                        return enumerator.Current.Point;
                    }
                    previousFrame = enumerator.Current;
                }
                // Calculate interpolation
                if (previousFrame != null && enumerator.Current != null)
                {
                    long ratio = previousFrame.Timestamp / enumerator.Current.Timestamp;
                    return PointTools.CalculateIntermediatePoint(previousFrame.Point, enumerator.Current.Point, ratio);
                }
            }

            return Point.Empty;
        }

        public event EventHandler<PenFoundEventArgs> PenFound;
        public event EventHandler<EventArgs> NoPenFound;


#if DEBUG
        public event EventHandler<DebugPictureEventArgs> DebugPicture;
#endif
    }

#if DEBUG
    public class DebugPictureEventArgs : EventArgs
    {
        public List<Bitmap> Pictures { get; private set; }
        public DebugPictureEventArgs(List<Bitmap> pics)
        {
            Pictures = pics;
        }
    }
#endif
}
