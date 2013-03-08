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
        static void Main1(string[] args)
        {
            int i = 0;
            ThreeChannelBitmap tci = new ThreeChannelBitmap();
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
            Console.Write(parser.Topl.X + " "+parser.Topl.Y);
            Console.Read();
        }

        static void Main2(string[] args)
        {
            //int i;
            //var thread = new Thread(VisualizerControl.Show);
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
            //thread.Join();
            //thread = new Thread(() =>
            //    {
            //        VisualizerControl.Show();
            //         Console.Write(VisualizerControl.Width + " x " + VisualizerControl.Height);
            //VisualizerControl.AddRect(100,100,200,200,Color.FromRgb(0,255,0));
            //Thread.Sleep(1000);
            //VisualizerControl.Close();
            //        Dispatcher.Run();
            //    });
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.IsBackground = true;
            //thread.Start();
            //Console.Read();
        }

        static void Main(string[] args)
        {
            var vc = new VisualizerControl();
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
                vc.Transparent = false;
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
