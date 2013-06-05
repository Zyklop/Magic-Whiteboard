using System;
using System.Runtime.InteropServices;

namespace InputEmulation
{
    public class DirectInput
    {
        public const ushort VK_1 = 0x02;
        public const ushort VK_2 = 0x03;
        public const ushort VK_3 = 0x04;
        public const ushort VK_4 = 0x05;
        public const ushort VK_5 = 0x06;
        public const ushort VK_6 = 0x07;
        public const ushort VK_7 = 0x08;
        public const ushort VK_8 = 0x09;
        public const ushort VK_9 = 0x0A;
        public const ushort VK_0 = 0x0B;
        public const ushort VK_MINUS = 0x0C;    /* - on main keyboard */
        public const ushort VK_EQUALS = 0x0D;
        public const ushort VK_BACK = 0x0E;    /* backspace */
        public const ushort VK_TAB = 0x0F;
        public const ushort VK_Q = 0x10;
        public const ushort VK_W = 0x11;
        public const ushort VK_E = 0x12;
        public const ushort VK_R = 0x13;
        public const ushort VK_T = 0x14;
        public const ushort VK_Y = 0x15;
        public const ushort VK_U = 0x16;
        public const ushort VK_I = 0x17;
        public const ushort VK_O = 0x18;
        public const ushort VK_P = 0x19;
        public const ushort VK_LBRACKET = 0x1A;
        public const ushort VK_RBRACKET = 0x1B;
        public const ushort VK_RETURN = 0x1C;    /* Enter on main keyboard */
        public const ushort VK_LCONTROL = 0x1D;
        public const ushort VK_A = 0x1E;
        public const ushort VK_S = 0x1F;
        public const ushort VK_D = 0x20;
        public const ushort VK_F = 0x21;
        public const ushort VK_G = 0x22;
        public const ushort VK_H = 0x23;
        public const ushort VK_J = 0x24;
        public const ushort VK_K = 0x25;
        public const ushort VK_L = 0x26;
        public const ushort VK_SEMICOLON = 0x27;
        public const ushort VK_APOSTROPHE = 0x28;
        public const ushort VK_GRAVE = 0x29;    /* accent grave */
        public const ushort VK_LSHIFT = 0x2A;
        public const ushort VK_BACKSLASH = 0x2B;
        public const ushort VK_Z = 0x2C;
        public const ushort VK_X = 0x2D;
        public const ushort VK_C = 0x2E;
        public const ushort VK_V = 0x2F;
        public const ushort VK_B = 0x30;
        public const ushort VK_N = 0x31;
        public const ushort VK_M = 0x32;
        public const ushort VK_COMMA = 0x33;
        public const ushort VK_PERIOD = 0x34;    /* . on main keyboard */
        public const ushort VK_SLASH = 0x35;    /* / on main keyboard */
        public const ushort VK_RSHIFT = 0x36;
        public const ushort VK_MULTIPLY = 0x37;    /* * on numeric keypad */
        public const ushort VK_LMENU = 0x38;    /* left Alt */
        public const ushort VK_SPACE = 0x39;
        public const ushort VK_CAPITAL = 0x3A;
        public const ushort VK_F1 = 0x3B;
        public const ushort VK_F2 = 0x3C;
        public const ushort VK_F3 = 0x3D;
        public const ushort VK_F4 = 0x3E;
        public const ushort VK_F5 = 0x3F;
        public const ushort VK_F6 = 0x40;
        public const ushort VK_F7 = 0x41;
        public const ushort VK_F8 = 0x42;
        public const ushort VK_F9 = 0x43;
        public const ushort VK_F10 = 0x44;
        public const ushort VK_F11 = 0x57;
        public const ushort VK_F12 = 0x58;
        public const ushort VK_NUMLOCK = 0x45;
        public const ushort VK_SCROLL = 0x46;    /* Scroll Lock */
        public const ushort VK_NUMPAD7 = 0x47;
        public const ushort VK_NUMPAD8 = 0x48;
        public const ushort VK_NUMPAD9 = 0x49;
        public const ushort VK_SUBTRACT = 0x4A;    /* - on numeric keypad */
        public const ushort VK_NUMPAD4 = 0x4B;
        public const ushort VK_NUMPAD5 = 0x4C;
        public const ushort VK_NUMPAD6 = 0x4D;
        public const ushort VK_ADD = 0x4E;    /* + on numeric keypad */
        public const ushort VK_NUMPAD1 = 0x4F;
        public const ushort VK_NUMPAD2 = 0x50;
        public const ushort VK_NUMPAD3 = 0x51;
        public const ushort VK_NUMPAD0 = 0x52;
        public const ushort VK_DECIMAL = 0x53;    /* . on numeric keypad */
        public const ushort VK_RMENU = 0xB8;    /* right Alt */
        public const ushort VK_PAUSE = 0xC5;    /* Pause */
        public const ushort VK_HOME = 0xC7;    /* Home on arrow keypad */
        public const ushort VK_UP = 0xC8;    /* UpArrow on arrow keypad */
        public const ushort VK_PGUP = 0xC9;    /* PgUp on arrow keypad */
        public const ushort VK_LEFT = 0xCB;    /* LeftArrow on arrow keypad */
        public const ushort VK_RIGHT = 0xCD;    /* RightArrow on arrow keypad */
        public const ushort VK_END = 0xCF;    /* End on arrow keypad */
        public const ushort VK_DOWN = 0xD0;    /* DownArrow on arrow keypad */
        public const ushort VK_NEXT = 0xD1;    /* PgDn on arrow keypad */
        public const ushort VK_INSERT = 0xD2;    /* Insert on arrow keypad */
        public const ushort VK_DELETE = 0xD3;    /* Delete on arrow keypad */
        public const ushort VK_LWIN = 0xDB;    /* Left Windows key */
        public const ushort VK_RWIN = 0xDC;    /* Right Windows key */

        public static void PressKey(char ch, bool press)
        {
            byte vk = WindowsAPI.VkKeyScan(ch);
            ushort scanCode = (ushort)WindowsAPI.MapVirtualKey(vk, 0);

            if (press)
                KeyDown(scanCode);
            else
                KeyUp(scanCode);
        }

        public static void KeyDown(ushort scanCode)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = WindowsAPI.INPUT_KEYBOARD;
            inputs[0].ki.dwFlags = WindowsAPI.KEYEVENTF_SCANCODE;
            inputs[0].ki.wScan = (ushort)(scanCode);

            uint intReturn = WindowsAPI.SendInput(1, inputs, Marshal.SizeOf(inputs[0]));
            if (intReturn != 1)
            {
                throw new Exception("Could not send key: " + scanCode);
            }
        }

        public static void KeyUp(ushort scanCode)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = WindowsAPI.INPUT_KEYBOARD;
            inputs[0].ki.wScan = scanCode;
            inputs[0].ki.dwFlags = WindowsAPI.KEYEVENTF_KEYUP;
            uint intReturn = WindowsAPI.SendInput(1, inputs, Marshal.SizeOf(inputs[0]));
            if (intReturn != 1)
            {
                throw new Exception("Could not send key: " + scanCode);
            }
        }
    }
    public class WindowsAPI
    {

        /// <summary>
        /// The GetForegroundWindow function returns a handle to the foreground window.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
          IntPtr hProcess,
          IntPtr lpBaseAddress,
          [Out()] byte[] lpBuffer,
          int dwSize,
          out int lpNumberOfBytesRead
         );

        public static void SwitchWindow(IntPtr windowHandle)
        {
            if (GetForegroundWindow() == windowHandle)
                return;

            IntPtr foregroundWindowHandle = GetForegroundWindow();
            uint currentThreadId = GetCurrentThreadId();
            uint temp;
            uint foregroundThreadId = GetWindowThreadProcessId(foregroundWindowHandle, out temp);
            AttachThreadInput(currentThreadId, foregroundThreadId, true);
            SetForegroundWindow(windowHandle);
            AttachThreadInput(currentThreadId, foregroundThreadId, false);

            while (GetForegroundWindow() != windowHandle)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hwndParent"></param>
        /// <param name="hwndChildAfter"></param>
        /// <param name="lpszClass"></param>
        /// <param name="lpszWindow"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern byte VkKeyScan(char ch);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);
     

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static int MakeLong(int low, int high)
        {
            return (high << 16) | (low & 0xffff);
        }

        [DllImport("User32.dll")]
        public static extern uint SendInput(uint numberOfInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] input, int structSize);

        [DllImport("user32.dll")]
        public static extern IntPtr GetMessageExtraInfo();

        public const int INPUT_MOUSE = 0;
        public const int INPUT_KEYBOARD = 1;
        public const int INPUT_HARDWARE = 2;
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const uint KEYEVENTF_UNICODE = 0x0004;
        public const uint KEYEVENTF_SCANCODE = 0x0008;
        public const uint XBUTTON1 = 0x0001;
        public const uint XBUTTON2 = 0x0002;
        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        public const uint MOUSEEVENTF_XDOWN = 0x0080;
        public const uint MOUSEEVENTF_XUP = 0x0100;
        public const uint MOUSEEVENTF_WHEEL = 0x0800;
        public const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
        public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        int dx;
        int dy;
        uint mouseData;
        uint dwFlags;
        uint time;
        IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        uint uMsg;
        ushort wParamL;
        ushort wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT
    {
        [FieldOffset(0)]
        public int type;
        [FieldOffset(4)] //*
        public MOUSEINPUT mi;
        [FieldOffset(4)] //*
        public KEYBDINPUT ki;
        [FieldOffset(4)] //*
        public HARDWAREINPUT hi;
    }
}
