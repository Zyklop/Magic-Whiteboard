using AForge.Imaging;
using AForge.Imaging.Filters;
using HSR.PresentationWriter.DataSources;
using HSR.PresentationWriter.Parser;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;

namespace HSR.PresentationWriter.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            AForgePenTracker t = new AForgePenTracker();
            Frame f1 = new Frame(1, new Bitmap(@"c:\temp\images\1_source16.bmp"));
            Frame f2 = new Frame(2, new Bitmap(@"c:\temp\images\2_overlay16.bmp"));
            t.Feed(f1);
            t.Feed(f2);
            t.Process();
        }
    }
}
