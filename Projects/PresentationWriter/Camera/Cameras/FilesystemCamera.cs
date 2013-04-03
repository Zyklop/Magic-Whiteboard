using HSR.PresWriter.Containers;
using HSR.PresWriter.IO;
using HSR.PresWriter.IO.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace HSR.PresWriter.IO.Cameras
{
    public class FilesystemCamera : ICamera
    {
        private const int TIMER_INTERVAL = 5000;
        private Timer tim = new Timer();
        private List<Bitmap> imagelist;
        private int index = 0;

        public FilesystemCamera(DirectoryInfo d)
        {
            this.imagelist = new List<Bitmap>();
            foreach(FileInfo i in d.GetFiles("*.jpg")) {
                Debug.WriteLine(i.FullName);
                this.imagelist.Add(new Bitmap(i.FullName));
            }
        }

        public void Start()
        {
            tim.Interval = TIMER_INTERVAL;
            tim.AutoReset = true;
            tim.Elapsed += delegate
            {
                if (FrameReady != null) FrameReady(this, new FrameReadyEventArgs(new VideoFrame(index, imagelist[index])));
                if (++index >= imagelist.Count) index = 0;
            };
            tim.Start();
        }

        public void Stop()
        {
            // do nothing
        }

        public VideoFrame GetLastFrame()
        {
            return new VideoFrame(index, imagelist[index]);
        }

        public event EventHandler<FrameReadyEventArgs> FrameReady;

        public void Dispose()
        {
            Stop();
        }
    }
}
