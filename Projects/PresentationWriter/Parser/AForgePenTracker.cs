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
        private const int MAX_FRAMEBUFFER_LENGTH = 2;
        private const int SAME_POINT_TRESHOLD = 2;
        private LinkedList<Frame> frameBuffer;
        /// <summary>
        /// TODO No threshold for size. Must be fixed eventually</summary>
        private Dictionary<long, Point> penPoints;

        public AForgePenTracker()
        {
            this.frameBuffer = new LinkedList<Frame>();
            this.penPoints = new Dictionary<long, Point>();
        }

        public void Process()
        {
            Bitmap previousImage;
            Bitmap currentImage;

            lock (this.frameBuffer) // lock, while accessing pictures
            {
                Frame previousFrame = this.frameBuffer.Last.Previous.Value;
                Frame currentFrame = this.frameBuffer.Last.Value;
                previousImage = new Bitmap(previousFrame.Image);
                currentImage = new Bitmap(currentFrame.Image);
            }

            // diff both images
            Difference differenceFilter = new Difference(previousImage);
            Bitmap diffImage = differenceFilter.Apply(currentImage);
            diffImage.Save(@"c:\temp\images\3_diff16.bmp");

            // convert to grayscale with focus on red parts
            Grayscale grayFilter = new Grayscale(1, 0, 0);
            Bitmap grayImage = grayFilter.Apply(diffImage);
            grayImage.Save(@"c:\temp\images\4_grey16.bmp");

            // make monochrome picture without noisy red parts
            IFilter thresholdFilter = new Threshold(50);
            Bitmap tresholdImage = thresholdFilter.Apply(grayImage);
            tresholdImage.Save(@"c:\temp\images\5_grey16.bmp");

            // count white blobs
            BlobCounter bc = new BlobCounter();
            bc.ProcessImage(tresholdImage);
            Rectangle[] rects = bc.GetObjectsRectangles();

            // only save found points if there are two of them
            if (rects.Length == 2)
            {
                lock (penPoints)
                {
                    Point previousPoint = penPoints.Values.Last();
                    Point newA = new Point(rects[0].X, rects[0].Y);
                    Point newB = new Point(rects[1].X, rects[1].Y);
                    double distanceA = getDistance(newA, previousPoint);
                    double distanceB = getDistance(newB, previousPoint);

                    if (distanceA < SAME_POINT_TRESHOLD)
                    {
                        Point currentPoint = newA;
                    }
                    if (distanceB < SAME_POINT_TRESHOLD)

                    {
                        Point currentPoint = newB;
                    }

                    if (distanceA > SAME_POINT_TRESHOLD && distanceB > SAME_POINT_TRESHOLD)
                    {
                        throw new Exception("previous point lost");
                    }
                }
            }

            // debug out
            foreach (Rectangle rect in rects)
            {
                Debug.WriteLine("{0}, {1}", rect.X, rect.Y);
            }
        }

        private double getDistance(Point p, Point q)
        {
            return Math.Sqrt(Math.Pow(q.X - p.X, 2) + Math.Pow(q.Y - p.Y, 2));
        }

        public void Feed(Frame frame)
        {
            if (frameBuffer.Count >= MAX_FRAMEBUFFER_LENGTH)
            {
                frameBuffer.RemoveFirst();
            }
            frameBuffer.AddLast(frame);
        }

        public Nullable<Point> GetLastPenPosition()
        {
            if (penPoints.Count > 0)
            {
                return penPoints.Values.Last();
            }
            return null;
        }

        public Nullable<Point> GetPenPosition(long timestamp)
        {
            if (penPoints.Count <= 0)
            {
                return null;
            }
            return null;
        }

        public event EventHandler<PenPositionEventArgs> PenMoved;

        public event EventHandler<PenPositionEventArgs> PenDetected;

        public event EventHandler<PenPositionEventArgs> PenLost;
    }
}
