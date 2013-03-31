using System;
using System.Runtime.InteropServices;

namespace HSR.PresWriter.IO
{
    /// <summary>
    /// Access to Win32 features</summary>
    public class NativeMethods
    {
        // TODO: Explicit 64bit Compatibility
        [DllImport("user32", EntryPoint = "SendMessage")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        public delegate void CallbackGetFrame(IntPtr hwnd, ref VIDEOHDR videoHeader);

        [StructLayout(LayoutKind.Sequential)]
        public struct VIDEOHDR
        {
            public IntPtr lpData;           // Pointer to locked data buffer
            public uint dwBufferLength;     // Length of data buffer
            public uint dwBytesUsed;        // Bytes actually used
            public uint dwTimeCaptured;     // Milliseconds from start of stream
            public uint dwUser;             // For client's use
            public uint dwFlags;            // Assorted flags (see defines)
            [MarshalAs(UnmanagedType.SafeArray)]
            byte[] dwReserved;              // Reserved for driver
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;

            public static BITMAPINFO Default
            {
                get
                {
                    BITMAPINFO bi = new BITMAPINFO();
                    bi.biSize = 40;
                    bi.biPlanes = 1;
                    bi.biBitCount = 24;
                    return bi;
                }
            }
            //public int[] ColorTable;
        }

        [DllImport("user32", EntryPoint = "SendMessage")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, CallbackGetFrame callback);

        [DllImport("avicap32.dll", EntryPoint = "capCreateCaptureWindowA", CharSet = CharSet.Unicode)]
        public static extern int capCreateCaptureWindowA(string lpszWindowName, int dwStyle, int X, int Y, int nWidth, int nHeight, int hwndParent, int nID);
    }
}
