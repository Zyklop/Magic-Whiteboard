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
    public class TimedFilesystemCamera : FilesystemCamera
    {
        public const int STANDARD_INTERVAL = 2000;
        private Timer _timer = new Timer();

        public TimedFilesystemCamera(DirectoryInfo d, int interval = STANDARD_INTERVAL)
            : base(d)
        {
            _timer.Interval = interval;
            _timer.AutoReset = true;
            _timer.Elapsed += delegate
            {
                this.Next();
            };
        }

        public new void Start()
        {
            base.Start();
            _timer.Start();
        }

        public new void Stop()
        {
            _timer.Stop();
            base.Stop();
        }
    }
}
