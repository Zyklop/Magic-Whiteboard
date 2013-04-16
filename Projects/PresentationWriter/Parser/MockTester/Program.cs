using System;
using System.Diagnostics;
using System.Threading;
using HSR.PresWriter.DataSources.Cameras;
using HSR.PresWriter.PenTracking;
using HSR.PresWriter.PenTracking.Images;
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
            Main4(args);
        }

        static void Main1(string[] args)
        {
            int i = 0;
            ThreeChannelBitmap tci = new ThreeChannelBitmap();
            var parser = new DataParser(new FilesystemCamera(new DirectoryInfo(@"C:\temp\aforge\inp")),WFVisuslizer.VisualizerControl.GetVisualizer());
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
            var cam = new FilesystemCamera(new DirectoryInfo(@"C:\temp\daforge\created"));
            cam.Start();
            var parser = new DataParser(cam,WFVisuslizer.VisualizerControl.GetVisualizer());
            parser.Start();
            Thread.Sleep(15000);
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
    }
}
