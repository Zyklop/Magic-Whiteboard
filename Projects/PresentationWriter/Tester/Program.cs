using AForge.Imaging;
using AForge.Imaging.Filters;
using HSR.PresWriter.IO.Cameras;
using HSR.PresWriter.IO.Events;
using HSR.PresWriter.PenTracking;
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
            FilesystemCamera camera = new FilesystemCamera(new DirectoryInfo(@"C:\temp\images\laser"));
            camera.FrameReady += delegate(object sender, FrameReadyEventArgs e)
            {
                Console.WriteLine(e.Frame.Number);
            };
            camera.Start();
            camera.Next();
            camera.Next();
            camera.Next();
            Console.ReadLine();
            camera.Stop();
        }
    }
}