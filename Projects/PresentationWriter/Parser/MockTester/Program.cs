using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using HSR.PresentationWriter.DataSources;
using HSR.PresentationWriter.Parser;
using HSR.PresentationWriter.Parser.Events;
using HSR.PresentationWriter.Parser.Images;
using Parser;
using Parser.Mock;
using WFVisuslizer;
using Color = System.Drawing.Color;

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
            var cameraConnector = new CameraConnector(new AForgeCamera());
            var parser = new DataParser(cameraConnector);
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
            Console.Write(parser.Topl.X + " "+parser.Topl.Y);
            Console.Read();
        }

        static void Main2(string[] args)
        {
            int i = 0;
            var cameraConnector = new MockCameraConnector();
            var parser = new DataParser(cameraConnector);
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
            Console.Write(parser.Topl.X + " " + parser.Topl.Y);
            Console.Read();
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
                    vc.ClearRects();
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
