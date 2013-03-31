using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectShowLib;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;
using HSR.PresWriter.Containers;
using HSR.PresWriter.IO.Events;

namespace HSR.PresWriter.IO.Cameras
{
    class DirectShowCamera : ICamera
    {
        // a small enum to record the graph state
        enum PlayState
        {
            Stopped,
            Paused,
            Running,
            Init
        };

        public event EventHandler<FrameReadyEventArgs> FrameReady;

        // Application-defined message to notify app of filtergraph events
        public const int WM_GRAPHNOTIFY = 0x8000 + 1;

        IVideoWindow videoWindow = null;
        IMediaControl mediaControl = null;
        IMediaEventEx mediaEventEx = null;
        IGraphBuilder graphBuilder = null;
        ICaptureGraphBuilder2 captureGraphBuilder = null;
        PlayState currentState = PlayState.Stopped;

        DsROTEntry rot = null;

        public DirectShowCamera()
        {

        }

        public void Start()
        {
            CaptureVideo();
        }

        public void Stop()
        {
            CloseInterfaces();
        }

        public VideoFrame GetLastFrame()
        {
            return null;
        }

        public void Dispose()
        {
            this.Stop();
        }

        public void CaptureVideo()
        {
            int hr = 0;
            IBaseFilter sourceFilter = null;

            try
            {
                // Get DirectShow interfaces
                GetInterfaces();

                // Attach the filter graph to the capture graph
                hr = this.captureGraphBuilder.SetFiltergraph(this.graphBuilder);
                DsError.ThrowExceptionForHR(hr);

                // Use the system device enumerator and class enumerator to find
                // a video capture/preview device, such as a desktop USB video camera.
                sourceFilter = FindCaptureDevice();

                // Add Capture filter to our graph.
                hr = this.graphBuilder.AddFilter(sourceFilter, "Video Capture");
                DsError.ThrowExceptionForHR(hr);

                // Render the preview pin on the video capture filter
                // Use this instead of this.graphBuilder.RenderFile
                hr = this.captureGraphBuilder.RenderStream(PinCategory.Preview, MediaType.Video, sourceFilter, null, null);
                DsError.ThrowExceptionForHR(hr);

                // Now that the filter has been added to the graph and we have
                // rendered its stream, we can release this reference to the filter.
                //Marshal.ReleaseComObject(sourceFilter);


                // Add our graph to the running object table, which will allow
                // the GraphEdit application to "spy" on our graph
                rot = new DsROTEntry(this.graphBuilder);


                // disable Auto focus (which is by default and set the focus to 0)
                var control = sourceFilter as IAMCameraControl;
                if (control != null)
                {
                    /*
                    control.Set(CameraControlProperty.Focus, Conf.Focus, CameraControlFlags.Manual);
                    control.Set(CameraControlProperty.Zoom, Conf.Zoom, CameraControlFlags.Manual);
                    control.Set(CameraControlProperty.Pan, Conf.Pan, CameraControlFlags.Manual);
                    control.Set(CameraControlProperty.Tilt, Conf.Tilt, CameraControlFlags.Manual);
                    control.Set(CameraControlProperty.Exposure, Conf.Exposure, CameraControlFlags.Manual);
                     */
                    control.Set(CameraControlProperty.Exposure, 1, CameraControlFlags.Manual);
                }

                var procAmp = sourceFilter as IAMVideoProcAmp;
                if (procAmp != null)
                {
                    Debug.WriteLine("gach");
                    /*
                    procAmp.Set(VideoProcAmpProperty.Brightness, Conf.Brightness, VideoProcAmpFlags.Manual);
                    procAmp.Set(VideoProcAmpProperty.Contrast, Conf.Contrast, VideoProcAmpFlags.Manual);
                    procAmp.Set(VideoProcAmpProperty.Saturation, Conf.Saturation, VideoProcAmpFlags.Manual);
                    procAmp.Set(VideoProcAmpProperty.Sharpness, Conf.Sharpness, VideoProcAmpFlags.Manual);
                    procAmp.Set(VideoProcAmpProperty.WhiteBalance, Conf.WhiteBalance, VideoProcAmpFlags.Manual);
                    procAmp.Set(VideoProcAmpProperty.BacklightCompensation, Conf.BacklightCompensation, VideoProcAmpFlags.Manual);
                    */
                    procAmp.Set(VideoProcAmpProperty.Saturation, 88, VideoProcAmpFlags.Manual);
                    procAmp.Set(VideoProcAmpProperty.Contrast, 0, VideoProcAmpFlags.Manual);
                    procAmp.Set(VideoProcAmpProperty.WhiteBalance, 1000, VideoProcAmpFlags.None);
                }


                // look into the properties
                int val;
                VideoProcAmpFlags flags;
                procAmp.Get(VideoProcAmpProperty.WhiteBalance, out val, out flags);
                Debug.WriteLine(val + "<->" + flags);

                // Start previewing video data
                hr = this.mediaControl.Run();
                DsError.ThrowExceptionForHR(hr);

                // Remember current state
                this.currentState = PlayState.Running;
            }
            catch
            {
                Debug.WriteLine("An unrecoverable error has occurred.");
            }
        }

        public IBaseFilter FindCaptureDevice()
        {
            DsDevice[] devices;
            object source;

            // Get all video input devices
            devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            // Take the first device
            DsDevice device = (DsDevice)devices[0];

            // Bind Moniker to a filter object
            Guid iid = typeof(IBaseFilter).GUID;
            device.Mon.BindToObject(null, null, ref iid, out source);

            // An exception is thrown if cast fail
            return (IBaseFilter)source;
        }

        public void GetInterfaces()
        {
            int hr = 0;

            // An exception is thrown if cast fail
            this.graphBuilder = (IGraphBuilder)new FilterGraph();
            this.captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
            this.mediaControl = (IMediaControl)this.graphBuilder;
            this.videoWindow = (IVideoWindow)this.graphBuilder;
            this.mediaEventEx = (IMediaEventEx)this.graphBuilder;

            hr = this.mediaEventEx.SetNotifyWindow(new IntPtr(0), WM_GRAPHNOTIFY, IntPtr.Zero);
            DsError.ThrowExceptionForHR(hr);
        }

        public void CloseInterfaces()
        {
            // Stop previewing data
            if (this.mediaControl != null)
                this.mediaControl.StopWhenReady();

            this.currentState = PlayState.Stopped;

            // Stop receiving events
            if (this.mediaEventEx != null)
                this.mediaEventEx.SetNotifyWindow(IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero);

            // Relinquish ownership (IMPORTANT!) of the video window.
            // Failing to call put_Owner can lead to assert failures within
            // the video renderer, as it still assumes that it has a valid
            // parent window.
            if (this.videoWindow != null)
            {
                this.videoWindow.put_Visible(OABool.False);
                this.videoWindow.put_Owner(IntPtr.Zero);
            }

            // Remove filter graph from the running object table
            if (rot != null)
            {
                rot.Dispose();
                rot = null;
            }

            // Release DirectShow interfaces
            Marshal.ReleaseComObject(this.mediaControl); this.mediaControl = null;
            Marshal.ReleaseComObject(this.mediaEventEx); this.mediaEventEx = null;
            Marshal.ReleaseComObject(this.videoWindow); this.videoWindow = null;
            Marshal.ReleaseComObject(this.graphBuilder); this.graphBuilder = null;
            Marshal.ReleaseComObject(this.captureGraphBuilder); this.captureGraphBuilder = null;
        }
    }
}
