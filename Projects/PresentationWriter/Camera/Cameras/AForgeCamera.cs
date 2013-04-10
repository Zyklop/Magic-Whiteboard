using System;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Diagnostics;
using HSR.PresWriter.IO.Events;
using HSR.PresWriter.Containers;


namespace HSR.PresWriter.IO.Cameras
{
    public class AForgeCamera : ICamera
    {
        private FilterInfoCollection videoCaptureDevices;
        private VideoCaptureDevice finalVideo;
        private int lastFrameNumber = 0;
        private long lastTimestamp = 0;
        private Bitmap lastBitmap = null;

        public bool IsRunning { get; protected set; }

        public event EventHandler<FrameReadyEventArgs> FrameReady;

        public AForgeCamera()
        {
            videoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            // TODO Choose camera device. We just use the first camera for now.
            finalVideo = new VideoCaptureDevice(videoCaptureDevices[0].MonikerString);
        }

        public void ShowConfigurationDialog()
        {
            finalVideo.DisplayPropertyPage(new IntPtr(0));
        }

        public void Start()
        {
            this.IsRunning = true;
            finalVideo.NewFrame += finalVideo_NewFrame;
            finalVideo.Start();
        }

        public void Stop()
        {
            finalVideo.NewFrame -= finalVideo_NewFrame;
            this.IsRunning = false;
            finalVideo.SignalToStop();
        }

        public VideoFrame GetLastFrame()
        {
            if (lastBitmap != null)
            {
                return new VideoFrame(lastFrameNumber, lastBitmap, lastTimestamp);
            }
            return null;
        }

        void finalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            lastTimestamp = CurrentMillis.Millis;
            lastFrameNumber++;
            lastBitmap = (Bitmap)eventArgs.Frame.Clone();

            if (FrameReady != null)
            {
                FrameReady(this, new FrameReadyEventArgs(GetLastFrame()));
            }

            //Debug.WriteLine("Got Frame {0}", lastFrameNumber);
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
