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
            CameraConfig();
            //TakeVideo();
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