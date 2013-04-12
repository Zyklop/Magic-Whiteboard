using System;
using System.Threading;
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
        
        static void Main(string[] args)
        {
            Main2(args);
        }

        static void Main1(string[] args)
        {
            int i = 0;
            ThreeChannelBitmap tci = new ThreeChannelBitmap();
            var parser = new DataParser(new FilesystemCamera(new DirectoryInfo(@"C:\temp\aforge\inp")));
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
            var cam = new FilesystemCamera(new DirectoryInfo(@"C:\temp\daforge\inpm"));
            cam.Start();
            var parser = new DataParser(cam);
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
    }
}
