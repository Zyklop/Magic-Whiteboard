using System.Runtime.InteropServices;

namespace HSR.PresentationWriter.DataSources
{
    /// <summary>
    /// Access to Win32 features</summary>
    class NativeMethods
    {
        // TODO: Explicit 64bit Compatibility
        [DllImport("user32", EntryPoint = "SendMessage")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        [DllImport("avicap32.dll", EntryPoint = "capCreateCaptureWindowA", CharSet = CharSet.Unicode)]
        public static extern int capCreateCaptureWindowA(string lpszWindowName, int dwStyle, int X, int Y, int nWidth, int nHeight, int hwndParent, int nID);
    }
}
