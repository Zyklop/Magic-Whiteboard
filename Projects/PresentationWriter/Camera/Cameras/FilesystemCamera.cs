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
        private List<Bitmap> _images = new List<Bitmap>();
        private DirectoryInfo _sourcePath;
        private int _currentFrameNumber;

        public bool IsRunning { get; protected set; }

        public FilesystemCamera(DirectoryInfo d)
        {
            _sourcePath = d;
        }

        public void Start()
        {
            IsRunning = true;

            // Load Files
            _currentFrameNumber = 0;
            foreach (FileInfo i in _sourcePath.GetFiles("*.*"))
            {
                switch(i.Extension)
                {
                    case ".jpg":
                    case ".png":
                    case ".bmp":
                        _images.Add(new Bitmap(i.FullName));
                        break;
                }
            }
        }

        public void Next()
        {
            if (FrameReady != null && ++_currentFrameNumber <= _images.Count) 
            {
                VideoFrame f = new VideoFrame(_currentFrameNumber, _images[_currentFrameNumber]);
                FrameReady(this, new FrameReadyEventArgs(f));
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public VideoFrame GetLastFrame()
        {
            return new VideoFrame(++_currentFrameNumber, _images[_currentFrameNumber]);
        }

        public event EventHandler<FrameReadyEventArgs> FrameReady;

        public void Dispose()
        {
            Stop();
        }
    }
}
