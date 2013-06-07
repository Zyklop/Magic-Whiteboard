using System;
using System.Runtime.InteropServices;

namespace HSR.PresWriter.InputEmulation
{
    public class VirtualKeys
    {
        #region Imports

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        internal static extern short VkKeyScan(char ch);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        #endregion

        public static uint Click()
        {
            var structure = new INPUT {mi = {dx = 0, dy = 0, mouseData = 0, dwFlags = 2}};
            var input2 = structure;
            input2.mi.dwFlags = 4;
            var pInputs = new INPUT[] {structure, input2};
            return SendInput(2, pInputs, Marshal.SizeOf(structure));
        }

        public static void SendKeyAsInput(System.Windows.Forms.Keys key)
        {
            var structure = new INPUT
                {
                    type = (int) InputType.INPUT_KEYBOARD,
                    ki = {wVk = (short) key, dwFlags = (int) KEYEVENTF.KEYDOWN, dwExtraInfo = GetMessageExtraInfo()}
                };

            var input2 = new INPUT
                {
                    type = (int) InputType.INPUT_KEYBOARD,
                    ki = {wVk = (short) key},
                    mi = {dwFlags = (int) KEYEVENTF.KEYUP}
                };
            input2.ki.dwExtraInfo = GetMessageExtraInfo();

            var pInputs = new INPUT[] {structure, input2};

            SendInput(2, pInputs, Marshal.SizeOf(structure));
        }

        public static void SendKeyAsInput(System.Windows.Forms.Keys key, int holdTime)
        {
            var INPUT1 = new INPUT
                {
                    type = (int) InputType.INPUT_KEYBOARD,
                    ki = {wVk = (short) key, dwFlags = (int) KEYEVENTF.KEYDOWN, dwExtraInfo = GetMessageExtraInfo()}
                };
            SendInput(1, new INPUT[] {INPUT1}, Marshal.SizeOf(INPUT1));

            keybd_event(0x41, 0, 0, 0);
            WaitForSingleObject((IntPtr) 0xACEFDB, (uint) holdTime);

            var INPUT2 = new INPUT
                {
                    type = (int) InputType.INPUT_KEYBOARD,
                    ki = {wVk = (short) key},
                    mi = {dwFlags = (int) KEYEVENTF.KEYUP}
                };
            INPUT2.ki.dwExtraInfo = GetMessageExtraInfo();
            SendInput(1, new INPUT[] {INPUT2}, Marshal.SizeOf(INPUT2));

        }

        public static void SendKeyDown(System.Windows.Forms.Keys key)
        {
            var INPUT1 = new INPUT
                {
                    type = (int) InputType.INPUT_KEYBOARD,
                    ki = {wVk = (short) key, dwFlags = (int) KEYEVENTF.KEYDOWN, dwExtraInfo = GetMessageExtraInfo()}
                };
            SendInput(1, new INPUT[] {INPUT1}, Marshal.SizeOf(INPUT1));
        }


        public static void SendKeyUp(System.Windows.Forms.Keys key)
        {
            var INPUT2 = new INPUT
                {
                    type = (int) InputType.INPUT_KEYBOARD,
                    ki = {wVk = (short) key},
                    mi = {dwFlags = (int) KEYEVENTF.KEYUP}
                };
            INPUT2.ki.dwExtraInfo = GetMessageExtraInfo();
            SendInput(1, new INPUT[] { INPUT2 }, Marshal.SizeOf(INPUT2));
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct INPUT
        {
            [FieldOffset(4)] public HARDWAREINPUT hi;
            [FieldOffset(4)] public KEYBDINPUT ki;
            [FieldOffset(4)] public MOUSEINPUT mi;
            [FieldOffset(0)] public int type;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [Flags]
        internal enum InputType
        {
            INPUT_MOUSE = 0,
            INPUT_KEYBOARD = 1,
            INPUT_HARDWARE = 2
        }

        [Flags]
        internal enum MOUSEEVENTF
        {
            MOVE = 0x0001, /* mouse move */
            LEFTDOWN = 0x0002, /* left button down */
            LEFTUP = 0x0004, /* left button up */
            RIGHTDOWN = 0x0008, /* right button down */
            RIGHTUP = 0x0010, /* right button up */
            MIDDLEDOWN = 0x0020, /* middle button down */
            MIDDLEUP = 0x0040, /* middle button up */
            XDOWN = 0x0080, /* x button down */
            XUP = 0x0100, /* x button down */
            WHEEL = 0x0800, /* wheel button rolled */
            MOVE_NOCOALESCE = 0x2000, /* do not coalesce mouse moves */
            VIRTUALDESK = 0x4000, /* map to entire virtual desktop */
            ABSOLUTE = 0x8000 /* absolute move */
        }

        [Flags]
        internal enum KEYEVENTF
        {
            KEYDOWN = 0,
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            UNICODE = 0x0004,
            SCANCODE = 0x0008,
        }
    }
}