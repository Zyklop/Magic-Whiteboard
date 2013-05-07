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
    public class AForgePenTracker : IPenTracker
    {
        /// <summary>
        /// Max count of frames to be included in pen discovering process.</summary>
        private const int MAX_FRAMEBUFFER_LENGTH = 3;
        // How much points we keep
        private const int MAX_POINTBUFFER_LENGTH = 10000;
        private IPictureProvider _source;
        private FixedSizedQueue<VideoFrame> _frameBuffer;
        private FixedSizedQueue<PointFrame> _penPoints;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(Environment.ProcessorCount);

        public FilterStrategy Strategy { get; set; }

        public AForgePenTracker(FilterStrategy strategy, IPictureProvider source)
        {
            _source = source;
            this.Strategy = strategy;
            this._frameBuffer = new FixedSizedQueue<VideoFrame>(MAX_FRAMEBUFFER_LENGTH);
            this._penPoints = new FixedSizedQueue<PointFrame>(MAX_POINTBUFFER_LENGTH);
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
            /* We allow #CPUs Pictures to be processed at the same time.
             * If there is no free logigal CPU, we discard the current frame
             */
            if (_semaphore.Wait(0))
            {
                var task = Task.Run(() =>
                {
                    try
                    {
                        processFrame(e.Frame);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                });

                // rethrow Exception if necessary
                if (task.Exception != null)
                {
                    throw task.Exception;
                }
            }
        }

        private void processFrame(VideoFrame currentFrame)
        {
            VideoFrame previousFrame;

            // Lock buffer for adding elements in dependency of queue length and preserving order.
            lock (this._frameBuffer) 
            {
                // We can only do our work if there is at least one frame in the buffer already
                if (this._frameBuffer.Count < 1)
                {
                    this._frameBuffer.Enqueue(currentFrame);
                    return;
                }
                else
                {
                    // we can do our work, since there is a previous frame.
                    previousFrame = this._frameBuffer.Last(); 
                    this._frameBuffer.Enqueue(currentFrame);
                }
            }

            // Status: We now have references to a previous and a current frame in the correct order.

            try
            {
                // Finding a new pen point and storing it must be atomic, because 
                // - findPenCandidates(..) chooses its search area dependently on the last found point
                // - findPen(..) accesses the previously found pointframe for interpolation
                lock (this._penPoints) // TODO Eventually a barrier would be better
                {
                    // Find pen candidates and evaluate them → finding candidates is expensive!
                    var candidates = findPenCandidates((Bitmap)previousFrame.Bitmap.Clone(), (Bitmap)currentFrame.Bitmap.Clone()); //TODO Fast fix against locking

                    // Evaluate the candidates (PenCandidate)
                    Point foundPoint = this.findPen(candidates);
                    if (foundPoint.IsEmpty)
                    {
                        if (this.NoPenFound != null)
                        {
                            this.NoPenFound(this, null);
                        }
                        return;
                    }

                    // Put the qualified result in a new point frame and fire event
                    PointFrame resultFrame = new PointFrame(currentFrame.Number, foundPoint, currentFrame.Timestamp);
                    this._penPoints.Enqueue(resultFrame);
                    if (this.PenFound != null)
                    {
                        this.PenFound(this, new PenPositionEventArgs(resultFrame));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error in Frame Processin: "+e.Message);
                // TODO Error Handling: Maybe we should catch everything for stability.
            }
        }

        /// <summary>
        /// This is the time consuming part. We try to find pen candidates in two pictures 
        /// by their filtered difference. We choose the search area in a intelligent way.
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        private IEnumerable<PenCandidate> findPenCandidates(Bitmap previous, Bitmap current)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            IEnumerable<PenCandidate> candidates = null;
            if (!_penPoints.IsEmpty) // TODO analyze timing. Discard too old images
            {
                // First, search near the last found point (in a rectangle of 100x100px)
                // This means, that a point could have moved 50px in every direction. TODO: analyze moving direction and velocity
                int areaX = _penPoints.Last().Point.X - 50;
                int areaY = _penPoints.Last().Point.Y - 50;
                Rectangle searchArea = new Rectangle(areaX, areaY, 100, 100);
                candidates = findPenCandidatesInArea(previous, current, searchArea);
            }

            // if we could not use the information of a previously tracked point, we search the whole image
            // WEAKNESS: if we detect one in our search area, but two are present on the whole picture, 
            // we give false feedback. Therefore, the searcharea must be choosen carefully
            if (candidates == null || candidates.Count() == 0)
            {
                candidates = findPenCandidatesInArea(previous, current, Rectangle.Empty);
            }

            long stage1 = sw.ElapsedMilliseconds;
            sw.Stop();
            if (stage1 >= 40)
            {
                Console.WriteLine("Pen Tracking Overtime: {0}", stage1);
            }

            return candidates;
        }

        /// <summary>
        /// This is the time consuming part. We try to find pen candidates in two pictures 
        /// by their filtered difference in a specified area.
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="current"></param>
        /// <returns></returns>
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
            Bitmap diffImage = this.Strategy.DifferenceFilter.Apply(current);
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

        private Point findPen(IEnumerable<PenCandidate> candidates)
        {
            // Return intermediate point
            switch (candidates.Count())
            {
                case 0:
                    // No pen found or pen is not moving
                    // TODO (0,0) is actually valid!
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
        /// <returns></returns>
        public PointFrame GetLastFrame()
        {
            if (_penPoints.Count > 0)
            {
                return _penPoints.Last();
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

            var enumerator = this._penPoints.GetEnumerator();
            while (enumerator.MoveNext() && timestamp < enumerator.Current.Timestamp)
            {
                if (timestamp == enumerator.Current.Timestamp)
                {
                    return enumerator.Current.Point;
                }
                previousFrame = enumerator.Current;
            }

            if (previousFrame != null && enumerator.Current != null)
            {
                long ratio = previousFrame.Timestamp / enumerator.Current.Timestamp;
                return PointTools.CalculateIntermediatePoint(previousFrame.Point, enumerator.Current.Point, ratio);
            }

            return Point.Empty;
        }

        public event EventHandler<PenPositionEventArgs> PenFound;
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
