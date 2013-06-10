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

        /// <summary>
        /// Check, if the Camera is enabled
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Enable the mirroring of the camera image
        /// </summary>
        public bool IsMirrored { get; set; }

        public event EventHandler<FrameReadyEventArgs> FrameReady;

        /// <summary>
        /// Use the first camera 
        /// </summary>
        public AForgeCamera()
        {
            _videoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            _finalVideo = new VideoCaptureDevice(_videoCaptureDevices[0].MonikerString);
        }

        /// <summary>
        /// Get a List of all connected cameras
        /// </summary>
        /// <returns>Dictionary with Moniker-String as key and the name as value</returns>
        public static Dictionary<string, string> GetCameras()
        {
            return new FilterInfoCollection(FilterCategory.VideoInputDevice).Cast<FilterInfo>().ToDictionary(fi => fi.MonikerString, fi => fi.Name);
        }

        /// <summary>
        /// Create a specified camera
        /// </summary>
        /// <param name="monikerString"></param>
        public AForgeCamera(string monikerString)
        {
            _finalVideo = new VideoCaptureDevice(monikerString);
        }

        /// <summary>
        /// Show camera properities
        /// </summary>
        public void ShowConfigurationDialog()
        {
            _finalVideo.DisplayPropertyPage(new IntPtr(0));
        }

        /// <summary>
        /// Start capturing images
        /// </summary>
        public void Start()
        {
            _finalVideo.Start();
            _finalVideo.NewFrame += finalVideo_NewFrame;
            IsRunning = true;
        }

        /// <summary>
        /// Stop the capture
        /// </summary>
        public void Stop()
        {
            _finalVideo.NewFrame -= finalVideo_NewFrame;
            IsRunning = false;
            _finalVideo.SignalToStop();
        }

        /// <summary>
        /// Get the last Image
        /// </summary>
        /// <returns>The last VideoFrame or null if no frame is avaliable</returns>
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

        /// <summary>
        /// Dispose the camera object
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}
