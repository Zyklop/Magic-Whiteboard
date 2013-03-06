using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace HSR.PresentationWriter.DataSources
{
    /// <summary>
    /// Used to access a webcam per AVICAP</summary>
    public class AvicapCamera : ICamera
    {
        #region AVICAP Constants

        public const int WM_USER = 1024;

        public const int WM_CAP_CONNECT = 1034;
        public const int WM_CAP_DISCONNECT = 1035;
        public const int WM_CAP_GET_FRAME = 1084;
        public const int WM_CAP_COPY = 1054;

        public const int WM_CAP_START = WM_USER;

        public const int WM_CAP_SET_CALLBACK_FRAME = WM_CAP_START + 5;
        public const int WM_CAP_DLG_VIDEOFORMAT = WM_CAP_START + 41;
        public const int WM_CAP_DLG_VIDEOSOURCE = WM_CAP_START + 42;
        public const int WM_CAP_DLG_VIDEODISPLAY = WM_CAP_START + 43;
        public const int WM_CAP_GET_VIDEOFORMAT = WM_CAP_START + 44;
        public const int WM_CAP_SET_VIDEOFORMAT = WM_CAP_START + 45;
        public const int WM_CAP_DLG_VIDEOCOMPRESSION = WM_CAP_START + 46;
        public const int WM_CAP_SET_PREVIEW = WM_CAP_START + 50;

        #endregion

        private int currentFrame = 0;
        private int mCapHwnd;

        public event EventHandler<FrameReadyEventArgs> FrameReady;

        /// <summary>
        /// Creates a camera instance. Device must be started, before
        /// the images can be accessed</summary>
        public AvicapCamera()
        {

        }

        /// <summary>
        /// Connect to physical device per AVICAP</summary>
        public void Start()
        {
            currentFrame = 0;
            // Setup a virtual capture window
            mCapHwnd = NativeMethods.capCreateCaptureWindowA("WebCap", 0, 0, 0, 0, 0, 0, 0);
            // Connect to the device
            NativeMethods.SendMessage(mCapHwnd, WM_CAP_CONNECT, 0, 0);
            NativeMethods.SendMessage(mCapHwnd, WM_CAP_SET_PREVIEW, 0, 0);
            NativeMethods.SendMessage(mCapHwnd, WM_CAP_SET_CALLBACK_FRAME, 0, FrameCallback);
        }

        /// <summary>
        /// Disconnect device</summary>
        public void Stop()
        {
            NativeMethods.SendMessage(mCapHwnd, WM_CAP_DISCONNECT, 0, 0);
            currentFrame = 0;
        }

        /// <summary>
        /// We capture on demand. Not every frame is captured.
        /// There is no streaming mechanism or 
        /// </summary>
        /// <returns>Last captured frame</returns>
        public VideoFrame GetLastFrame()
        {
            // Get next frame from camera with avicap
            NativeMethods.SendMessage(mCapHwnd, WM_CAP_GET_FRAME, 0, 0);

            /**
             * We measure time after awaiting the picture.
             * Waiting times:
             * - Min 4ms
             * - Max 45ms (probably the framerate of 22fps)
             * - Avg 16ms
             */
            long timestamp = CurrentMillis.Millis;

            // Save it in clipboard
            NativeMethods.SendMessage(mCapHwnd, WM_CAP_COPY, 0, 0);

            return new VideoFrame(++currentFrame, GetClipboardImage(), timestamp);
        }

        /// <summary>
        /// Because AviCap saves everything in the Windows Clipboard, we have to get 
        /// our captures from there. Since the Clipboard is only accessible from a 
        /// STA-Thread, we create one if we aren't already one.
        /// </summary>
        /// <returns>Last image in Clipboard</returns>
        public static Image GetClipboardImage()
        {
            Image ret = null;
            ThreadStart method = delegate()
            {
                System.Windows.Forms.IDataObject dataObject = System.Windows.Forms.Clipboard.GetDataObject();
                if (dataObject != null && dataObject.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap))
                {
                    ret = (System.Drawing.Bitmap)dataObject.GetData(System.Windows.Forms.DataFormats.Bitmap);
                }
            };
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                Thread thread = new Thread(method);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            else
            {
                method();
            }
            return ret;
        }

        public void Dispose()
        {
            this.Stop();
        }

        void FrameCallback(IntPtr hWnd, ref NativeMethods.VIDEOHDR vhdr)
        {
            if (hWnd == IntPtr.Zero) return;

            long timestamp = CurrentMillis.Millis;
            NativeMethods.SendMessage(mCapHwnd, WM_CAP_COPY, 0, 0);

            if (FrameReady != null)
            {
                //IntPtr data = vhdr.lpData;
                //NativeMethods.BITMAPINFO binfo = NativeMethods.BITMAPINFO.Default;
                //Bitmap bmp = new Bitmap(binfo.biWidth, binfo.biHeight, binfo.biWidth * binfo.biBitCount / 8, System.Drawing.Imaging.PixelFormat.Format24bppRgb, data);
                VideoFrame f = new VideoFrame(++currentFrame, GetClipboardImage(), timestamp);
                FrameReady(this, new FrameReadyEventArgs(f));
            }
        }
    }
}
