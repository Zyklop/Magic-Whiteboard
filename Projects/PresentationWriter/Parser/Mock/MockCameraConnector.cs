using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using HSR.PresWriter.Containers;
using HSR.PresWriter.IO;
using HSR.PresWriter.IO.Events;
using HSR.PresWriter.PenTracking;
using HSR.PresWriter.PenTracking.Events;
using HSR.PresWriter.PenTracking.Images;
using System.Drawing;
using Timer = System.Timers.Timer;

namespace Parser.Mock
{

    public class MockCameraConnector : IPictureProvider
    {
        private System.Timers.Timer tim = new Timer();
        private List<Bitmap> imagelist;
        private int index=0;

        public MockCameraConnector()
        {
            imagelist = new List<Bitmap>();
            for (int i = 1; i < 50; i++)
            {
                try
                {
                    imagelist.Add(new Bitmap(@"C:\temp\aforge\inp\img" + i + ".jpg"));
                }
                catch (ArgumentException e)
                {
                }
            }
            Start();
        }

        private void Start()
        {
            tim.Interval = 5000;
            tim.AutoReset = true;
            tim.Elapsed += delegate
                {
                    if (NewImage != null) NewImage(this, new NewImageEventArgs {NewImage = imagelist[index]});
                    if (FrameReady != null) FrameReady(this, new FrameReadyEventArgs(new VideoFrame(index, imagelist[index])));
                    if (++index >= imagelist.Count) index = 0;
                    //Thread.Sleep(0);
                };
            tim.Start();
        }

        public event EventHandler<NewImageEventArgs> NewImage;
        public event EventHandler<FrameReadyEventArgs> FrameReady;
    }
}
