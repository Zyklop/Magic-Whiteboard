using AForge.Imaging;
using AForge.Imaging.Filters;
using HSR.PresentationWriter.DataSources;
using HSR.PresentationWriter.Parser.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HSR.PresentationWriter.Parser
{
    public class AForgePenTracker : IPenTracker
    {
        /// <summary>
        /// Max count of frames to be included in pen discovering process.</summary>
        private const int MAX_FRAMEBUFFER_LENGTH = 3;
        private const int MAX_POINTBUFFER_LENGTH = 3;
        private FixedSizedQueue<VideoFrame> frameBuffer;
        private FixedSizedQueue<PointFrame> penPoints;
        private int currentFrameNumber;

        public FilterStrategy Strategy { get; set; }

        public AForgePenTracker(FilterStrategy strategy)
        {
            this.Strategy = strategy;
            this.frameBuffer = new FixedSizedQueue<VideoFrame>(MAX_FRAMEBUFFER_LENGTH);
            this.penPoints = new FixedSizedQueue<PointFrame>(MAX_POINTBUFFER_LENGTH);
            this.currentFrameNumber = 0;
        }

        public async Task<PointFrame> ProcessAsync(VideoFrame currentFrame)
        {
            VideoFrame previousFrame;
            // access correct framebuffer positions, cost: 50ns (source: http://www.informit.com/guides/content.aspx?g=dotnet&seqNum=600 )
            lock (frameBuffer) 
            {
                // There must be at least one element in the queue already.
                if (this.frameBuffer.Count < 1)
                {
                    this.frameBuffer.Enqueue(currentFrame);
                    return null;
                }
                else
                {
                    previousFrame = this.frameBuffer.Last();
                    this.frameBuffer.Enqueue(currentFrame);
                }
            }

            // Here begins the expensive part!
            Point foundPoint;
            try
            {
                foundPoint = findPen(previousFrame.Bitmap, currentFrame.Bitmap);
                // Check result and frame it
                if (!foundPoint.IsEmpty)
                {
                    // interpolate timestamps, since we found an interpolated pen point
                    long timestamp = previousFrame.Timestamp + (currentFrame.Timestamp - previousFrame.Timestamp) / 2;
                    PointFrame resultFrame = new PointFrame(Interlocked.Increment(ref currentFrameNumber), foundPoint, timestamp);
                    this.penPoints.Enqueue(resultFrame);
                    if (this.PenMoved != null)
                    {
                        this.PenMoved(this, new PenPositionEventArgs(resultFrame));
                    }
                    return resultFrame;
                }
            }
            catch (Exception e)
            {
                // TODO Error Handling
            }

            return null;
        }

        /// <summary>
        /// ATTENTION: Bitmap a is overwritten because of a performance gain.
        /// this makes the previous picture unusable for a second pass!
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        private Point findPen(Bitmap previous, Bitmap current)
        {
            // calculate difference image
            Strategy.DifferenceFilter.OverlayImage = previous;
            Bitmap diff = Strategy.DifferenceFilter.Apply(current);
            //b.Save(@"c:\temp\images\r1_diff16.bmp");

            // translate red parts to gray image
            Bitmap gray = Strategy.GrayFilter.Apply(diff);
            //a.Save(@"c:\temp\images\r2_grey16.bmp");

            // treshold the gray image
            Bitmap treshold = Strategy.ThresholdFilter.Apply(gray);

#if DEBUG
            if (DebugPicture != null)
            {
                DebugPicture(this, new DebugPictureEventArgs(new List<Bitmap>() { diff, gray, treshold }));
            }
#endif

            // count white blobs (ev. kann man den thresholdFilter wegschmeissen, siehe ctr parameter von BlobCounter)
            Strategy.BlobCounter.ProcessImage(treshold);
            Rectangle[] r = Strategy.BlobCounter.GetObjectsRectangles();

            // Return intermediate point
            switch (r.Length)
            {
                case 0:
                    return Point.Empty;
                case 1:
                    return PointTools.CalculateCenterPoint(r[0]);
                case 2:
                    return PointTools.CalculateCenterPoint(
                        PointTools.CalculateCenterPoint(r[0]), 
                        PointTools.CalculateCenterPoint(r[1]));
                default:
                    throw new Exception("TODO: Error Handling: more than two points are bad! Wrong Camera adjustment!");
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

        public event EventHandler<PenPositionEventArgs> PenMoved;

        public event EventHandler<PenPositionEventArgs> PenDetected;

        public event EventHandler<PenPositionEventArgs> PenLost;


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
