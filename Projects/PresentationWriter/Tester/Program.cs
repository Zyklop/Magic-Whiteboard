using HSR.PresentationWriter.DataSources;
using System;
using System.Threading;

namespace HSR.PresentationWriter.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            AForgeCamera cam = new AForgeCamera();
            cam.FrameReady += cam_FrameReady;
            cam.Start();

            Thread.Sleep(1000);
            cam.GetLastFrame().Image.Save(@"C:\temp\special.jpg");
            Thread.Sleep(1000);
            cam.GetLastFrame().Image.Save(@"C:\temp\special2.jpg");

            cam.Stop();
        }

        private static void cam_FrameReady(object sender, FrameReadyEventArgs e)
        {
            e.Frame.Image.Save(@"C:\temp\gach" + e.Frame.Number + ".jpg");
        }
    }
}
