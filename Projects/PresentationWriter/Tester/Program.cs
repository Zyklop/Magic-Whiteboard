using HSR.PresentationWriter.DataSources;
using HSR.PresentationWriter.Parser;
using System;
using System.Collections.Generic;
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
            List<VideoFrame> l = new List<VideoFrame>() {
                new VideoFrame(1, new Bitmap(@"c:\temp\images\1_frame16.bmp"), 100),
                new VideoFrame(2, new Bitmap(@"c:\temp\images\2_frame16.bmp"), 900),
                new VideoFrame(3, new Bitmap(@"c:\temp\images\3_frame16.bmp"), 930),
                new VideoFrame(4, new Bitmap(@"c:\temp\images\4_frame16.bmp"), 960),
            };

            AForgePenTracker t = new AForgePenTracker();
            foreach(VideoFrame f in l)
            {
                t.Process(f);
                PointFrame p = t.GetLastFrame();
                if (p == null)
                {
                    Debug.WriteLine("P: no frame");
                }
                else
                {
                    Debug.WriteLine("P: {0}, {1}", p.Point.X, p.Point.Y);
                }
            }
        }
    }
}
