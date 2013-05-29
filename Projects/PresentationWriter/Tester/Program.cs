using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using HSR.PresWriter.Containers;
using HSR.PresWriter.IO.Cameras;
using HSR.PresWriter.IO.Events;
using HSR.PresWriter.PenTracking;
using HSR.PresWriter.PenTracking.Events;
using HSR.PresWriter.PenTracking.Strategies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HSR.PresWriter.Tester
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateBackMappingPicture();
        }

        public static void TestQuadRecognition()
        {
            Bitmap image = new Bitmap(@"c:\temp\grid.png");
            // define quadrilateral's corners
            List<IntPoint> corners = new List<IntPoint>();
            corners.Add(new IntPoint(266, 480-410));
            corners.Add(new IntPoint(522, 480-353));
            corners.Add(new IntPoint(533, 480-147));
            corners.Add(new IntPoint(266, 480-167));
            // create filter
            QuadrilateralTransformation filter = new QuadrilateralTransformation(corners, 1024, 768);
            // apply the filter
            Bitmap newImage = filter.Apply(image);
            newImage.Save(@"c:\temp\grid-transformed.png");
        }

        public static void CreateBackMappingPicture()
        {
            Bitmap target = new Bitmap(640, 480);
            Bitmap source = new Bitmap(640, 480);

            int i = 0;
            for (int x = 0; x < source.Width; x++)
            {
                for (int y = 0; y < source.Height; y++ )
                {
                    int r, g, b;
                    r = x % 256;
                    b = y % 256;
                    g = (x - r) / 16;
                    Color c = Color.FromArgb(r, g, b);
                    source.SetPixel(x, y, c);
                }
            }
            
            source.Save(@"c:\temp\mapping.bmp");

            List<IntPoint> corners = new List<IntPoint>();
            corners.Add(new IntPoint(266, 480 - 410));
            corners.Add(new IntPoint(522, 480 - 353));
            corners.Add(new IntPoint(533, 480 - 147));
            corners.Add(new IntPoint(266, 480 - 167));


            BackwardQuadrilateralTransformation filter = new BackwardQuadrilateralTransformation(source, corners);
            // apply the filter
            Bitmap newImage = filter.Apply(target);
            newImage.Save(@"c:\temp\mappingback.bmp");

        }

        public static void TrackPenOnLibrary()
        {
            Console.WriteLine("Starting...");

            TimedFilesystemCamera camera = new TimedFilesystemCamera(new DirectoryInfo(@"c:\temp\live\cap-127"), 1);
            AForgePenTracker tracker = new AForgePenTracker(new WhiteLedStrategy(), camera);

            camera.FrameReady += delegate(object o, FrameReadyEventArgs e) {
                Console.WriteLine("Cam Frame: {0}", e.Frame.Number);
            };

            tracker.PenFound += delegate(object o, PenPositionEventArgs e) {
                PointFrame p = e.Frame;
                String outOfOrder = "";
                if (e.IsOutOfOrder)
                {
                    outOfOrder = "(out of order)";
                }
                Console.WriteLine("Found {0} at {1},{2} {3}", p.Number, p.Point.X, p.Point.Y, outOfOrder);
            };

            tracker.Start();
            camera.Start();

            Console.WriteLine("Tracker and Camera Started.");
            Console.ReadLine();
        }

        public static void CameraConfig()
        {
            AForgeCamera camera = new AForgeCamera();
            camera.ShowConfigurationDialog();
        }

        public static void TakeVideo()
        {
            String saveDir = @"C:\temp\images\video";
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }

            AForgeCamera camera = new AForgeCamera();
            camera.FrameReady += delegate(object o, FrameReadyEventArgs e)
            {
                Console.WriteLine("Frame {0}", e.Frame.Number);
                e.Frame.Bitmap.Save(Path.Combine(saveDir,"pic-"+e.Frame.Number+".png"));
            };

            camera.Start();
            Console.ReadLine();
            camera.Stop();
        }

        public static void PenTrackingPerformance()
        {
            FilesystemCamera camera = new FilesystemCamera(new DirectoryInfo(@"C:\temp\images\laser"));
            camera.FrameReady += delegate(object o, FrameReadyEventArgs e) {
                Console.WriteLine("got image nr {0}.", e.Frame.Number);
            };

            AForgePenTracker tracker = new AForgePenTracker(new RedLaserStrategy(), camera);
            tracker.PenFound += delegate(object o, PenPositionEventArgs e)
            {
                Console.WriteLine("got point nr {0} ({1},{2}).", e.Frame.Number, e.Frame.Point.X, e.Frame.Point.Y);
            };

            camera.Start();
            tracker.Start();

            for (int i = 0; i < 50; i++)
            {
                camera.Next();
            }

            Console.ReadLine();
            tracker.Stop();
            camera.Stop();
        }
    }
}