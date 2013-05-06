using System;
using System.Diagnostics;
using System.Threading;
using HSR.PresWriter.DataSources.Cameras;
using HSR.PresWriter.PenTracking;
using HSR.PresWriter.PenTracking.Images;
using HSR.PresWriter.PenTracking.Strategies;
using InputEmulation;
using Visualizer;
using WFVisuslizer;
using Color = System.Drawing.Color;
using HSR.PresWriter.PenTracking.Events;
using HSR.PresWriter.IO.Cameras;
using System.IO;

namespace MockTester
{
    class Program
    {
        private static VirtualCamera _virtualCam;

        static void Main(string[] args)
        {
            Main2(args);
        }

        static void Main1(string[] args)
        {
            int i = 0;
            ThreeChannelBitmap tci = new ThreeChannelBitmap();
            var parser = new DataParser(new TimedFilesystemCamera(new DirectoryInfo(@"C:\temp\aforge\inp")),WFVisuslizer.VisualizerControl.GetVisualizer());
            parser.Start();
            Thread.Sleep(10000);
            //cameraConnector.NewImage += delegate(object sender, NewImageEventArgs e)
            //    {
            //        if (i>0)
            //        {
            //            (tci - e.NewImage).GetVisual().Save(@"C:\temp\gach" + i + ".jpg");
            //        }
                    
            //        tci = e.NewImage;
            //        i++;
            //    };
            Console.Read();
        }

        static void Main2(string[] args)
        {
            int i = 0;
            //var cam = new AForgeCamera();
            //cam.IsMirrored = true;
            //var visualizerControl = WFVisuslizer.VisualizerControl.GetVisualizer();
            var cam = new TimedFilesystemCamera(new DirectoryInfo(@"C:\temp\daforge\inpv2"));
            var visualizerControl = new VisualizerDummy();
            var parser = new DataParser(cam,visualizerControl);
            parser.Start();
            cam.Start();
            Thread.Sleep(1000);
            //cameraConnector.NewImage += delegate(object sender, NewImageEventArgs e)
            //    {
            //        if (i>0)
            //        {
            //            (tci - e.NewImage).GetVisual().Save(@"C:\temp\gach" + i + ".jpg");
            //        }

            //        tci = e.NewImage;
            //        i++;
            //    };
            parser.PenPositionChanged += NewPoint;
            Console.Read();
        }

        private static void NewPoint(object sender, PenPositionEventArgs e)
        {
            Console.WriteLine("Pen found at: " + e.Frame.Point.X + " / " + e.Frame.Point.Y);
            if (!e.Frame.Point.IsEmpty) ;
            //Mouse.MoveMouseAbsolute(e.Frame.Point.X,e.Frame.Point.Y);
            var r = new Random();
            _virtualCam.AddRect(e.Frame.Point.X + r.Next(-5,5)*20, e.Frame.Point.Y + r.Next(-5,5)*20,25,25, Color.Red);
            _virtualCam.Draw();

        }

        private static void Main3(string[] args)
        {
            var vc = VisualizerControl.GetVisualizer();
            Thread.Sleep(1000);
            //var thread = new Thread(vc.Show);
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.IsBackground = true;
            //thread.Start();
            var thread2 = new Thread(() =>
                {
                    vc.Show();
                    vc.Transparent = true;
                    Console.Write(vc.Width + " x " + vc.Height);
                    vc.AddRect(100, 100, 200, 200, Color.GreenYellow);
                    Thread.Sleep(1000);
                    vc.Clear();
                    Thread.Sleep(1000);
                    vc.Transparent = false;
                    Thread.Sleep(1000);
                    vc.AddRect(500, 100, 200, 200, Color.Red);
                    Thread.Sleep(1000);
                    vc.Close();
                    //    Dispatcher.Run();
                });
            thread2.SetApartmentState(ApartmentState.STA);
            thread2.Start();
            Console.Read();
        }

        static void Main4(string[] args)
        {
            _virtualCam = new VirtualCamera();
            _virtualCam.Start();
            var parser = new DataParser(_virtualCam, _virtualCam);
            parser.Start();
            parser.PenPositionChanged += NewPoint;
            while (true)
            {
                Thread.Sleep(10000);
                //_virtualCam.Clear();
                _virtualCam.AddRect(500, 500, 25, 25, Color.Red);
                _virtualCam.AddRect(530, 530, 25, 25, Color.Black);
                _virtualCam.Draw();
                Thread.Sleep(30);
                //_virtualCam.Clear();
                _virtualCam.AddRect(500, 500, 25, 25, Color.Black);
                _virtualCam.AddRect(530, 530, 25, 25, Color.Red);
                _virtualCam.Draw();
                Debug.WriteLine("Drew Point");
            }
            Console.Read();
        }

        private static void Main5(string[] args)
        {
            var t = new Touch(1, Touch.FeedbackMode.DEFAULT);
            t.Touchdown(500,500);
            for (int i = 0; i < 90; i++)
            {
                t.Hold();
                Thread.Sleep(30);
            }
            t.Release();
            var p = new Pen(1, Pen.FeedbackMode.DEFAULT);
            //p.Hover(200, 200);
            Thread.Sleep(1000);
            p.Touchdown(400, 400);
            for (int i = 0; i < 90; i++)
            {
                t.Hold();
                Thread.Sleep(30);
            }
            p.Release();
            Thread.Sleep(1000);
            p.OutOfRange();
            Console.Read();
        }

        static void Main6(string[] args)
        {
            _virtualCam = new VirtualCamera();
            _virtualCam.Start();
            var calib = new SimpleAForgeCalibrator(_virtualCam, _virtualCam);
            var pt = new AForgePenTracker(new RedLaserStrategy(), _virtualCam);
            var parser = new DataParser(calib, pt);
            parser.Start();
            parser.PenPositionChanged += NewPoint;
            while (true)
            {
                Thread.Sleep(10000);
                //_virtualCam.Clear();
                _virtualCam.AddRect(500, 500, 15, 15, Color.Red);
                _virtualCam.AddRect(530, 530, 15, 15, Color.Black);
                _virtualCam.Draw();
                Thread.Sleep(30);
                //_virtualCam.Clear();
                _virtualCam.AddRect(500, 500, 15, 15, Color.Black);
                _virtualCam.AddRect(530, 530, 15, 15, Color.Red);
                _virtualCam.Draw();
                Debug.WriteLine("Drew Point");
            }
            Console.Read();
        }

        static void Main7(string[] args)
        {
            var cam = new TimedFilesystemCamera(new DirectoryInfo(@"C:\temp\daforge\inph"));
            var visualizerControl = new VisualizerDummy();
            cam.Start();
            var calib = new SimpleAForgeCalibrator(cam, visualizerControl);
            var pt = new AForgePenTracker(new RedLaserStrategy(), cam);
            var parser = new DataParser(calib, pt);
            parser.Start();
            parser.PenPositionChanged += NewPoint;
            Console.Read();
            Console.WriteLine("again");
            Thread.Sleep(1000);
            parser.Start();
            Console.Read();
        }
    }
}
