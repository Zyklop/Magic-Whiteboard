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
        private const int MAX_POINTBUFFER_LENGTH = 3;
        private FixedSizedQueue<VideoFrame> frameBuffer;
        private FixedSizedQueue<PointFrame> penPoints;

        public FilterStrategy Strategy { get; set; }

        public AForgePenTracker(FilterStrategy strategy, IPictureProvider provider)
        {
            _provider = provider;
            this.Strategy = strategy;
            this.frameBuffer = new FixedSizedQueue<VideoFrame>(MAX_FRAMEBUFFER_LENGTH);
            this.penPoints = new FixedSizedQueue<PointFrame>(MAX_POINTBUFFER_LENGTH);
        }

        public void Start()
        {
            _provider.FrameReady += ProcessAsync;
        }

        public void Stop()
        {
            _provider.FrameReady -= ProcessAsync;
        }

        public async void ProcessAsync(object sender, FrameReadyEventArgs frameReadyEventArgs)
        {
            VideoFrame previousFrame;
            var currentFrame = frameReadyEventArgs.Frame;
            // Lock buffer for adding elements in dependency of queue length
            lock (this.frameBuffer) 
            {
                // We can only do our work if there is at least one frame in the buffer already
                if (this.frameBuffer.Count < 1)
                {
                    this.frameBuffer.Enqueue(currentFrame);
                    return;
                }
                else
                {
                    previousFrame = this.frameBuffer.Last();
                    this.frameBuffer.Enqueue(currentFrame);
                }
            }

            try
            {
                // Find pen candidates and evaluate them → finding candidates is expensive!
                var candidates = findPenCandidates((Bitmap) previousFrame.Bitmap.Clone(), (Bitmap) currentFrame.Bitmap.Clone()); //TODO Fast fix against locking

                // Finding a new pen point and storing it must be atomic, because findPen(..) accesses the previously found pointframe for interpolation
                lock (this.penPoints)
                {
                    // Evaluate the candidates (PenCandidate)
                    Point foundPoint = this.findPen(candidates);

                    
                    if (foundPoint.IsEmpty)
                    {
                        return;
                    }

                    // Put the qualified result in a new point frame and fire event
                    PointFrame resultFrame = new PointFrame(currentFrame.Number + 1, foundPoint, currentFrame.Timestamp);
                    this.penPoints.Enqueue(resultFrame);
                    if (this.PenFound != null)
                    {
                        this.PenFound(this, new PenPositionEventArgs(resultFrame));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                // TODO Error Handling: Maybe we should catch everything for bug containment.
            }
        }

        /// <summary>
        /// This is the time consuming part. We try to find pen candidates in two pictures 
        /// by their filtered difference.
        /// ATTENTION: Bitmap previous is overwritten because of a performance gain. This makes 
        /// the previous picture unusable for a second pass!!!
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        private IEnumerable<PenCandidate> findPenCandidates(Bitmap previous, Bitmap current)
        {
            // calculate difference image
            Strategy.DifferenceFilter.OverlayImage = previous;
            Bitmap diffImage = Strategy.DifferenceFilter.Apply(current);
            //b.Save(@"c:\temp\images\r1_diff16.bmp");

            // translate red parts to gray image
            Bitmap grayImage = Strategy.GrayFilter.Apply(diffImage);
            //a.Save(@"c:\temp\images\r2_grey16.bmp");

            // treshold the gray image
            Bitmap tresholdImage = Strategy.ThresholdFilter.Apply(grayImage);

#if DEBUG
            if (DebugPicture != null)
            {
                DebugPicture(this, new DebugPictureEventArgs(new List<Bitmap>() { diffImage, grayImage, tresholdImage }));
            }
#endif

            // count white blobs (TODO ev. kann man den thresholdFilter wegschmeissen, siehe ctr parameter von BlobCounter)
            Strategy.BlobCounter.ProcessImage(tresholdImage);

            // frame found blobs and add information
            var candidates = 
                from r in Strategy.BlobCounter.GetObjectsRectangles()
                select new PenCandidate()
                {
                    Rectangle = r,
                    WeightedCenter = PointTools.CalculateCenterPoint(r) // TODO more precise weightening
                };

            return candidates;
        }

        private Point findPen(IEnumerable<PenCandidate> candidates)
        {
            // Return intermediate point
            switch (candidates.Count())
            {
                case 0:
                    // No pen found or pen is not moving
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
            if (penPoints.Count > 0)
            {
                return penPoints.Last();
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

            var enumerator = this.penPoints.GetEnumerator();
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


#if DEBUG
        public event EventHandler<DebugPictureEventArgs> DebugPicture;
        private IPictureProvider _provider;
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
