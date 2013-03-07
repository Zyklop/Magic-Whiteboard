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
using System.Threading.Tasks;

namespace HSR.PresentationWriter.Parser
{
    public class AForgePenTracker : IPenTracker
    {
        /// <summary>
        /// Max count of frames to be included in pen discovering process.</summary>
        private const int MAX_FRAMEBUFFER_LENGTH = 3;
        private const int MAX_POINTBUFFER_LENGTH = 3;
        private LinkedList<VideoFrame> frameBuffer;
        private LinkedList<PointFrame> penPoints;
        private int currentFrameNumber;

        public AForgePenTracker()
        {
            this.frameBuffer = new LinkedList<VideoFrame>();
            this.penPoints = new LinkedList<PointFrame>();
            this.currentFrameNumber = 0;
            this.initFilters();
        }

        private Bitmap previousBitmap, currentBitmap;
        /// <summary>
        /// Process a new frame.
        /// That means queuing it an finding a new 
        /// </summary>
        public void Process(VideoFrame frame)
        {
            queue(frame);

            // if we have less than 2 images, we cant do anything
            if (this.frameBuffer.Count < 2)
            {
                return;
            }

            VideoFrame previousFrame = this.frameBuffer.Last.Previous.Value;
            VideoFrame currentFrame = this.frameBuffer.Last.Value;

            // interpolate timestamps
            long timestamp = previousFrame.Timestamp;
            timestamp += (currentFrame.Timestamp - previousFrame.Timestamp) / 2;

            previousBitmap = previousFrame.Bitmap;
            // We need to clone becaue we overwrite it in the pen tracking process,
            // but the frame is still needed in next iteration
            currentBitmap = (Bitmap)currentFrame.Bitmap.Clone();

            // get intermediate point between diff image
            try
            {
                Point p = findPen(previousBitmap, currentBitmap);
                if (!p.IsEmpty)
                {
                    queue(new PointFrame(++currentFrameNumber, p, timestamp));
                }
            }
            catch (Exception ex)
            { }
        }

        private Difference differenceFilter;
        private Grayscale grayFilter;
        private Threshold thresholdFilter;
        private BlobCounter blobCounter;
        private void initFilters()
        {
            differenceFilter = new Difference();
            grayFilter = new Grayscale(1, 0, 0);
            thresholdFilter = new Threshold(50);
            blobCounter = new BlobCounter();
        }

        private Point findPen(Bitmap a, Bitmap b)
        {
            //long time1 = CurrentMillis.Millis;

            // calculate difference
            differenceFilter.OverlayImage = a;
            differenceFilter.ApplyInPlace(b);
            //b.Save(@"c:\temp\images\r1_diff16.bmp");

            // translate red parts to gray image
            a = grayFilter.Apply(b);
            //a.Save(@"c:\temp\images\r2_grey16.bmp");

            // treshold the gray image
            thresholdFilter.ApplyInPlace(a);

#if DEBUG
            if (DebugPicture != null)
            {
                DebugPicture(this, new DebugPictureEventArgs(new List<Bitmap>() { b, a }));
            }
#endif

            //tresholdImage.Save(@"c:\temp\images\r3_treshold16.bmp");

            // count white blobs (ev. kann man den thresholdFilter wegschmeissen, siehe ctr parameter)
            blobCounter.ProcessImage(a);
            Rectangle[] r = blobCounter.GetObjectsRectangles();

            //long time2 = CurrentMillis.Millis;

            //StreamWriter streamWriter = new StreamWriter(@"c:\temp\gach.csv", true);
            //streamWriter.WriteLine("{0};{1}", time1, time2);
            //streamWriter.Close();

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
                    throw new Exception("TODO: Error Handling: more than two points are bad!");
            }
        }

        private void queue(VideoFrame frame)
        {
            if (frameBuffer.Count >= MAX_FRAMEBUFFER_LENGTH)
            {
                frameBuffer.RemoveFirst();
            }
            frameBuffer.AddLast(frame);
        }

        private void queue(PointFrame frame)
        {
            if (this.penPoints.Count >= MAX_POINTBUFFER_LENGTH)
            {
                this.penPoints.RemoveFirst();
            }
            this.penPoints.AddLast(frame);
        }

        public PointFrame GetLastFrame()
        {
            if (penPoints.Count > 0)
            {
                return penPoints.Last.Value;
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
            LinkedListNode<PointFrame> current = this.penPoints.First;
            while (current != null)
            {
                if (timestamp == current.Value.Timestamp)
                {
                    return current.Value.Point;
                }
                if (timestamp > current.Value.Timestamp && current.Next != null)
                {
                    Point currentPoint = current.Value.Point;
                    long currentTime = current.Value.Timestamp;
                    Point nextPoint = current.Next.Value.Point;
                    long nextTime = current.Next.Value.Timestamp;
                    long ratio = currentTime / nextTime;
                    return PointTools.CalculateIntermediatePoint(currentPoint, nextPoint, ratio);
                }
                current = current.Next;
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
