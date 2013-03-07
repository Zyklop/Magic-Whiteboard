using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Diagnostics;


namespace HSR.PresentationWriter.DataSources
{
    public class AForgeCamera : ICamera
    {
        private FilterInfoCollection videoCaptureDevices;
        private VideoCaptureDevice finalVideo;
        private int lastFrameNumber = 0;
        private long lastTimestamp = 0;
        private Bitmap lastBitmap = null;

        public event EventHandler<FrameReadyEventArgs> FrameReady;

        public AForgeCamera()
        {
            videoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            finalVideo = new VideoCaptureDevice(videoCaptureDevices[0].MonikerString);
        }

        public void ShowConfigurationDialog()
        {
            finalVideo.DisplayPropertyPage(new IntPtr(0));
        }

        public void Start()
        {
            finalVideo.NewFrame += new NewFrameEventHandler(finalVideo_NewFrame);
            finalVideo.Start();
        }

        public void Stop()
        {
            finalVideo.NewFrame -= new NewFrameEventHandler(finalVideo_NewFrame);
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

            Debug.WriteLine("Got Frame {0}", lastFrameNumber);
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
