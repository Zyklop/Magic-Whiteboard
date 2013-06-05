using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InputEmulation
{
    class KeyBoardEvent
    {

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public static void KeyDown(Keys key)
        {
            keybd_event((byte)key, 0, 0, 0);
        }

        public static void KeyUp(Keys key)
        {
            keybd_event((byte)key, 0, 0x7F, 0);
        }
    }
}
