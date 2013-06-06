using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InputEmulation
{
    public class Mouse
    {
        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        internal struct MouseInput
        {
            public int X;
            public int Y;
            public int MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        internal struct Input
        {
            public int Type;
            public MouseInput MouseInput;
        }

        internal static class NativeMethods
        {
            internal const int InputMouse = 0;

            internal const int MouseEventMove = 0x01;
            internal const int MouseEventLeftDown = 0x02;
            internal const int MouseEventLeftUp = 0x04;
            internal const int MouseEventRightDown = 0x08;
            internal const int MouseEventRightUp = 0x10;
            internal const int MouseEventAbsolute = 0x8000;
            internal const int MouseeventfWheel = 0x0800;
            internal const int WhMouseLl = 14;
            internal const int WheelUp = -120;
            internal const int WheelDown = 120;

            private static bool _lastLeftDown;

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern uint SendInput(uint numInputs, Input[] inputs, int size);

            /// <summary>
            /// Retrieves the cursor's position, in screen coordinates.
            /// </summary>
            /// <see>See MSDN documentation for further information.</see>
            [DllImport("user32.dll")]
            internal static extern bool GetCursorPos(out POINT lpPoint);
        }

        /// <summary>
        /// Move the mouse to an absolute position
        /// </summary>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        public static void MoveMouseAbsolute(int positionX, int positionY)
        {
            var i = new Input[1];

            // move the mouse to the position specified
            i[0] = new Input
                {
                    Type = NativeMethods.InputMouse,
                    MouseInput =
                        {
                            X = (positionX*65535)/Screen.PrimaryScreen.Bounds.Width,
                            Y = (positionY*65535)/Screen.PrimaryScreen.Bounds.Height,
                            Flags = NativeMethods.MouseEventAbsolute | NativeMethods.MouseEventMove
                        }
                };

            // send it off
            var result = NativeMethods.SendInput(1, i, Marshal.SizeOf(i[0]));
            if (result == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Move the mouse to a relatove position
        /// </summary>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        public static void MoveMouseRelative(int positionX, int positionY)
        {
            var i = new Input[1];

            // move the mouse to the position specified
            i[0] = new Input
                {
                    Type = NativeMethods.InputMouse,
                    MouseInput = {X = positionX, Y = positionY, Flags = NativeMethods.MouseEventMove}
                };

            // send it off
            uint result = NativeMethods.SendInput(1, i, Marshal.SizeOf(i[0]));
            if (result == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Click the mouse
        /// </summary>
        /// <param name="left">left mouse button?</param>
        /// <param name="up">release or press? </param>
        public static void ClickEvent(bool left, bool up)
        {
            var i = new Input[1];

            // move the mouse to the position specified
            i[0] = new Input {Type = NativeMethods.InputMouse};
            if (left)
            {
                i[0].MouseInput.Flags = (uint) (up ? NativeMethods.MouseEventLeftUp : NativeMethods.MouseEventLeftDown);
            }
            else
            {
                i[0].MouseInput.Flags = (uint) (up ? NativeMethods.MouseEventRightUp : NativeMethods.MouseEventRightDown);
            }
            // send it off
            uint result = NativeMethods.SendInput(1, i, Marshal.SizeOf(i[0]));
            if (result == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Move the mousewheel
        /// </summary>
        /// <param name="up"></param>
        public static void WheelEvent(bool up)
        {
            var i = new Input[1];

            // move the mouse to the position specified
            i[0] = new Input {Type = NativeMethods.InputMouse, MouseInput = {Flags = NativeMethods.MouseeventfWheel}};
            i[0].MouseInput.MouseData = up ? NativeMethods.WheelUp : NativeMethods.WheelDown;
            // send it off
            uint result = NativeMethods.SendInput(1, i, Marshal.SizeOf(i[0]));
            if (result == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            NativeMethods.GetCursorPos(out lpPoint);
            return lpPoint;
        }
    }

    public class Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; private set; }

        public int Y { get; private set; }
    }
}
