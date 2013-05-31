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
            TrackPenOnLibrary();
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
            filter.UseInterpolation = false;
            // apply the filter
            Bitmap newImage = filter.Apply(image);
            newImage.Save(@"c:\temp\grid-transformed.png");
        }

        public static void CreateBackMappingPicture()
        {
            Bitmap target = new Bitmap(640, 480);
            Bitmap source = new Bitmap(640, 480);

            const int size = 1000;
            const double ratio = 1.0 / size;
            const double saturation = 1.0;
            Color[,] colors = new Color[size, size];
            for (int x = 0; x < source.Width; x++)
            {
                double lightness = 1.0 - x * ratio;
                for (int y = 0; y < source.Height; y++ )
                {
                    double hue = y * ratio;
                    //colors[x, y] = FromHSL(hue, saturation, lightness);
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

            tracker.PenFound += delegate(object o, PenFoundEventArgs e) {
                PointFrame p = e.Frame;
                Console.WriteLine("-> Found {0} at {1},{2}", p.Number, p.Point.X, p.Point.Y);
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
            FilesystemCamera camera = new FilesystemCamera(new DirectoryInfo(@"c:\temp\live\cap-127"));
            camera.FrameReady += delegate(object o, FrameReadyEventArgs e) {
                Console.WriteLine("got image nr {0}.", e.Frame.Number);
            };

            AForgePenTracker tracker = new AForgePenTracker(new RedLaserStrategy(), camera);
            tracker.PenFound += delegate(object o, PenFoundEventArgs e)
            {
                Console.WriteLine("got point nr {0} ({1},{2}).", e.Frame.Number, e.Frame.Point.X, e.Frame.Point.Y);
            };

            camera.Start();
            tracker.Start();

            for (int i = 0; i < 50; i++)
            {
                camera.Next();
                Thread.Sleep(40); // every 40ms a picture
            }

            Console.ReadLine();
            tracker.Stop();
            camera.Stop();
        }
    }
}