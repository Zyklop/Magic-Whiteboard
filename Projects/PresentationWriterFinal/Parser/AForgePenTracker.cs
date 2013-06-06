using AForge.Imaging.Filters;
using HSR.PresWriter.Common.Containers;
using HSR.PresWriter.Common.IO;
using HSR.PresWriter.PenTracking.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PresWriter.Common.IO.Events;

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
    internal class AForgePenTracker : IPenTracker
    {
        private readonly IPictureProvider _source;
        private readonly FixedSizedQueue<VideoFrame> _frameBuffer;
        private readonly FixedSizedQueue<PointFrame> _penPoints;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public FilterStrategy Strategy { get; set; }

        public AForgePenTracker(FilterStrategy strategy, IPictureProvider source)
        {
            _source = source;
            Strategy = strategy;
            SearchAreaRadius = 50;
            _frameBuffer = new FixedSizedQueue<VideoFrame>(3);   // 3 video frames are used for discovering pen points
            _penPoints = new FixedSizedQueue<PointFrame>(10000); // 10000 points are kept
        }

        public void Start()
        {
            _source.FrameReady += OnTrackPen;
        }

        public void Stop()
        {
            _source.FrameReady -= OnTrackPen;
        }

        private void OnTrackPen(object sender, FrameReadyEventArgs e)
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
                        ProcessFrame(e.Frame);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                });

                // rethrow Exception if necessary
                if (task.Exception != null)
                {
                    Logger.Log(task.Exception);
                }
            }
        }

        private void ProcessFrame(VideoFrame currentFrame)
        {
            // We can only do our work if there is at least one frame in the buffer already
            if (_frameBuffer.Count < 1)
            {
                // But we keep the frame for the next incoming processing request
                _frameBuffer.Enqueue(currentFrame);
                return;
            }

            // We can do our work, since there is a previously taken frame.
            VideoFrame previousFrame = _frameBuffer.Last(); 
            _frameBuffer.Enqueue(currentFrame);

            // Status: 
            // - We now have references to a previous and a current video frame in the correct order.
            // - Additionally we have some previous points location (not necessarily matching to the previous video frame).
            try
            {
                // Reference to the last found point is needed to search more efficiently.
                // Efficiently means: Arround the last found point
                PointFrame previousPoint = _penPoints.LastOrDefault();

                // At first we search for the pen near the old one (if there was one)
                List<PenCandidate> candidates = null;
                if (previousPoint != null)
                {
                    candidates = FindPenCandidatesInArea(
                         (Bitmap)previousFrame.Bitmap.Clone(),
                         (Bitmap)currentFrame.Bitmap.Clone(),
                         GetEstimatedSearchArea(previousPoint)
                     ).ToList();
                }

                // If there is no previous point (previousPoint == null) or we couldn't use the information 
                // of a previously tracked point (candidates == 0), we search the whole image.
                // WEAKNESS: if we detect one in our search area, but two are present on the whole picture, 
                // we give FALSE feedback. Therefore, the searcharea must be choosen carefully
                if (previousPoint == null || !candidates.Any())
                {
                    candidates = FindPenCandidatesInArea(
                        (Bitmap)previousFrame.Bitmap.Clone(), 
                        (Bitmap)currentFrame.Bitmap.Clone(), 
                        Rectangle.Empty
                    ).ToList();
                }

                // Evaluate all the pen candidates for valid pen positions
                Point foundPoint = FindPen(candidates);

                // TODO empty may be a valid point (0,0)
                if (foundPoint.IsEmpty)
                {
                    if (NoPenFound != null)
                    {
                        NoPenFound(this, null);
                    }
                    return;
                }

                // Put the qualified result in a new point frame and queue it to the result list
                var resultFrame = new PointFrame(currentFrame.Number, foundPoint, currentFrame.Timestamp);
                _penPoints.Enqueue(resultFrame);

                if (PenFound != null)
                {
                    PenFound(this, new PenFoundEventArgs(resultFrame));
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        /// <summary>
        /// At first we try to search near the last found point (in a rectangle of 100x100px)
        /// This means, that a point could have moved 50px in every direction. TODO: analyze moving direction and velocity
        /// </summary>
        /// <param name="lastFoundPoint"></param>
        /// <returns></returns>
        private Rectangle GetEstimatedSearchArea(PointFrame lastFoundPoint)
        {
            var areaX = lastFoundPoint.Point.X - SearchAreaRadius;
            var areaY = lastFoundPoint.Point.Y - SearchAreaRadius;
            return new Rectangle(areaX, areaY, 2*SearchAreaRadius, 2*SearchAreaRadius);
        }

        /// <summary>
        /// This is the time consuming part. We try to find pen candidates in two pictures 
        /// by their filtered difference in a specified area.
        /// </summary>
        /// <param name="previous">Previous video picture</param>
        /// <param name="current">Current video picture</param>
        /// <param name="searchArea"></param>
        /// <returns>Candidates</returns>
        private IEnumerable<PenCandidate> FindPenCandidatesInArea(Bitmap previous, Bitmap current, Rectangle searchArea)
        {
            if (!searchArea.IsEmpty)
            {
                var cropFilter = new Crop(searchArea);
                previous = cropFilter.Apply(previous);
                current = cropFilter.Apply(current);
            }

            // calculate difference image
            Strategy.DifferenceFilter.OverlayImage = previous;
            var diffImage = current; 

            // translate red parts to gray image
            var grayImage = Strategy.GrayFilter.Apply(diffImage);

            // treshold the gray image
            var tresholdImage = Strategy.ThresholdFilter.Apply(grayImage);

            // count white blobs (TODO ev. kann man den thresholdFilter wegschmeissen, siehe ctr parameter von BlobCounter)
            Strategy.BlobCounter.ProcessImage(tresholdImage);

            // frame found blobs and add information
            var rawCandidates = Strategy.BlobCounter.GetObjectsRectangles();
            var candidates = new List<PenCandidate>();
            foreach (var r in rawCandidates)
            {
                r.Offset(searchArea.Location); // Adjust Blob Location to search area
                candidates.Add(new PenCandidate
                    {
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
        private Point FindPen(List<PenCandidate> candidates)
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
                    var candidateOne = candidates.ElementAt(0);
                    var candidateTwo = candidates.ElementAt(1);
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
            lock (_penPoints)
            {
                if (_penPoints.Count > 0)
                {
                    return _penPoints.Last();
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

            lock (_penPoints)
            {
                List<PointFrame>.Enumerator enumerator = _penPoints.ToList().GetEnumerator();
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

        public int SearchAreaRadius { get; set; }
    }
}
