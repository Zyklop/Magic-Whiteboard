using HSR.PresentationWriter.DataSources;
using System.Threading;

namespace HSR.PresentationWriter.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Camera cam = new Camera();
            cam.Start();
            cam.GetLastFrame().Image.Save(@"C:\temp\gach1.jpg");
            Thread.Sleep(500); 
            cam.GetLastFrame().Image.Save(@"C:\temp\gach2.jpg");
            cam.Stop();
        }
    }
}
