using HSR.PresentationWriter.DataSources;
using HSR.PresentationWriter.Parser;
using HSR.PresentationWriter.Parser.Strategies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HSR.PresentationWriter.Tester
{
    class Program
    {
        public static void Main(string[] args)
        {
            AForgeCamera c = new AForgeCamera();
            c.FrameReady += c_FrameReady;
            c.Start();
        }

        private static void c_FrameReady(object sender, FrameReadyEventArgs e)
        {
            DirectoryInfo d = new DirectoryInfo(@"C:\temp\images\light3");
            e.Frame.Bitmap.Save(@"C:\temp\images\light3\cap-" + e.Frame.Timestamp + ".png");
        }
    }
}

//        private static Bitmap trackingBitmap;
//        public static void Main(string[] args)
//        {
//            trackingBitmap = new Bitmap(800,600);
//            DirectoryInfo d = new DirectoryInfo(@"C:\temp\images\dot-red");
//            int i = 1;

//            AForgePenTracker tracker = new AForgePenTracker(new RedLaserStrategy());
//            tracker.PenMoved += tracker_PenMoved;
//#if DEBUG
//            tracker.DebugPicture += tracker_DebugPicture;
//#endif

//            foreach (FileInfo info in d.GetFiles())
//            {
//                VideoFrame frame = new VideoFrame(i, new Bitmap(info.FullName), i*40);
//                Task<PointFrame> task = tracker.ProcessAsync(frame);
//                task.Wait();
//                PointFrame p = task.Result;

//                if (p == null)
//                {
//                    Debug.WriteLine("P: no frame");
//                }
//                else
//                {
//                    Debug.WriteLine("P: {0}, {1}", p.Point.X, p.Point.Y);
//                }
//            }

//            trackingBitmap.Save(@"C:\temp\images\result-"+CurrentMillis.Millis+".bmp");
//        }

//    private static Point previousPoint = Point.Empty;
//    private static void tracker_PenMoved(object sender, Parser.Events.PenPositionEventArgs e)
//    {
//        Graphics g = Graphics.FromImage(trackingBitmap);
//        if (!previousPoint.IsEmpty)
//        {
            
//            g.DrawLine(Pens.Red, previousPoint.X, previousPoint.Y, e.Frame.Point.X, e.Frame.Point.Y);
//        }
//        else
//        {
//            g.DrawEllipse(Pens.Green, e.Frame.Point.X, e.Frame.Point.Y, 2, 2);
//        }
//        previousPoint = e.Frame.Point;
//    }

//#if DEBUG
//        private static int counter = 0;
//        private static void tracker_DebugPicture(object sender, DebugPictureEventArgs e)
//        {
//            counter++;
//            e.Pictures[0].Save(@"C:\temp\images\temp\img-" + counter + "-a.bmp");
//            e.Pictures[1].Save(@"C:\temp\images\temp\img-" + counter + "-b.bmp");
//        }
//#endif
//    }
//}
