using AForge.Imaging;
using AForge.Imaging.Filters;
using HSR.PresentationWriter.DataSources;
using HSR.PresentationWriter.Parser.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        }

        /// <summary>
        /// Process newest two frames
        /// </summary>
        public void Process()
        {
            // if we have less than 2 images, we cant do anything
            if (this.frameBuffer.Count < 2)
            {
                return;
            }

            VideoFrame previousFrame;
            VideoFrame currentFrame;

            lock (this.frameBuffer)
            {
                previousFrame = this.frameBuffer.Last.Previous.Value;
                currentFrame = this.frameBuffer.Last.Value;
            }
            // interpolate timestamp
            long timestamp = previousFrame.Timestamp;
            timestamp += (currentFrame.Timestamp - previousFrame.Timestamp) / 2;

            Bitmap previousBitmap = new Bitmap(previousFrame.Image);
            Bitmap currentBitmap = new Bitmap(currentFrame.Image);

            Point p = findPen(previousBitmap, currentBitmap);
            if (!p.IsEmpty)
            {
                if (this.penPoints.Count >= MAX_POINTBUFFER_LENGTH)
                {
                    this.penPoints.RemoveFirst();
                }
                this.penPoints.AddLast(new PointFrame(++currentFrameNumber, p, timestamp));
            }
        }

        private Point findPen(Bitmap a, Bitmap b)
        {
            Difference differenceFilter = new Difference(a);
            Bitmap diffImage = differenceFilter.Apply(b);
            diffImage.Save(@"c:\temp\images\r1_diff16.bmp");

            Grayscale grayFilter = new Grayscale(1, 0, 0);
            Bitmap grayImage = grayFilter.Apply(diffImage);
            grayImage.Save(@"c:\temp\images\r2_grey16.bmp");

            IFilter thresholdFilter = new Threshold(50);
            Bitmap tresholdImage = thresholdFilter.Apply(grayImage);
            tresholdImage.Save(@"c:\temp\images\r3_treshold16.bmp");

            // count white blobs
            BlobCounter bc = new BlobCounter();
            bc.ProcessImage(tresholdImage);
            Rectangle[] r = bc.GetObjectsRectangles();

            switch (r.Length)
            {
                case 0:
                    return Point.Empty;
                case 1:
                    return getCenterPoint(r[0]);
                case 2:
                    return getCenterPoint(getCenterPoint(r[0]), getCenterPoint(r[1]));
                default:
                    throw new Exception("TODO: Error Handling: more than two points are bad!");
            }
        }

        private Point getCenterPoint(Rectangle r)
        {
            return new Point(r.X + r.Width / 2, r.Y + r.Height);
        }

        private Point getCenterPoint(Point a, Point b)
        {
            // Always floor the results, thats conservative
            int x = a.X + ((b.X - a.X) / 2);
            int y = a.Y + ((b.Y - a.Y) / 2);
            return new Point(x, y);
        }

        private Point getIntermediatePoint(Point a, Point b, double ratio = 0.5)
        {
            int x = (int)Math.Round(a.X + ((b.X - a.X) * ratio));
            int y = (int)Math.Round(a.Y + ((b.Y - a.Y) * ratio));
            return new Point(x, y);
        }

        public void Feed(VideoFrame frame)
        {
            if (frameBuffer.Count >= MAX_FRAMEBUFFER_LENGTH)
            {
                frameBuffer.RemoveFirst();
            }
            frameBuffer.AddLast(frame);
        }

        public PointFrame GetLastFrame()
        {
            if (penPoints.Count > 0)
            {
                return penPoints.Last.Value;
            }
            return null;
        }

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
                    return getIntermediatePoint(currentPoint, nextPoint, ratio);
                }
                current = current.Next;
            }
            return Point.Empty;
        }

        public event EventHandler<PenPositionEventArgs> PenMoved;

        public event EventHandler<PenPositionEventArgs> PenDetected;

        public event EventHandler<PenPositionEventArgs> PenLost;
    }
}
