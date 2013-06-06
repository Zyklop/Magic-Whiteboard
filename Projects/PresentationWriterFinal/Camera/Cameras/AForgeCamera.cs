using System;
using System.Collections.Generic;
using System.Linq;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using HSR.PresWriter.Common;
using HSR.PresWriter.Common.Containers;
using PresWriter.Common.IO.Events;

namespace HSR.PresWriter.DataSources.Cameras
{
    public class AForgeCamera : ICamera
    {
        private readonly FilterInfoCollection _videoCaptureDevices;
        private readonly VideoCaptureDevice _finalVideo;
        private int _lastFrameNumber;
        private long _lastTimestamp;
        private Bitmap _lastBitmap;
        private readonly Mirror _filter = new Mirror( false, true );

        public bool IsRunning { get; protected set; }

        public bool IsMirrored { get; set; }

        public event EventHandler<FrameReadyEventArgs> FrameReady;

        public AForgeCamera()
        {
            _videoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            _finalVideo = new VideoCaptureDevice(_videoCaptureDevices[0].MonikerString);
        }

        public static Dictionary<string, string> GetCameras()
        {
            return new FilterInfoCollection(FilterCategory.VideoInputDevice).Cast<FilterInfo>().ToDictionary(fi => fi.MonikerString, fi => fi.Name);
        }

        public AForgeCamera(string monikerString)
        {
            _finalVideo = new VideoCaptureDevice(monikerString);
        }

        public void ShowConfigurationDialog()
        {
            _finalVideo.DisplayPropertyPage(new IntPtr(0));
        }

        public void Start()
        {
            IsRunning = true;
            _finalVideo.NewFrame += finalVideo_NewFrame;
            _finalVideo.Start();
        }

        public void Stop()
        {
            _finalVideo.NewFrame -= finalVideo_NewFrame;
            IsRunning = false;
            _finalVideo.SignalToStop();
        }

        public VideoFrame GetLastFrame()
        {
            if (_lastBitmap != null)
            {
                return new VideoFrame(_lastFrameNumber, _lastBitmap, _lastTimestamp);
            }
            return null;
        }

        void finalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            _lastTimestamp = CurrentMillis.Millis;
            _lastFrameNumber++;
            _lastBitmap = (Bitmap)eventArgs.Frame.Clone();
            if (IsMirrored)
            {
                _filter.ApplyInPlace(_lastBitmap);
            }
            if (FrameReady != null)
            {
                FrameReady(this, new FrameReadyEventArgs(GetLastFrame()));
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
