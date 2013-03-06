using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HSR.PresentationWriter.Parser;
using HSR.PresentationWriter.Parser.Events;
using HSR.PresentationWriter.Parser.Images;
using Parser;
using Parser.Mock;

namespace MockTester
{
    class Program
    {
        static void Main(string[] args)
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
    }
}
