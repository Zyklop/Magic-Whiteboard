using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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

        public static void MoveMouseAbsolute(int positionX, int positionY)
        {
            Input[] i = new Input[1];

            // move the mouse to the position specified
            i[0] = new Input();
            i[0].Type = NativeMethods.InputMouse;
            i[0].MouseInput.X = (positionX*65535)/Screen.PrimaryScreen.Bounds.Width;
            i[0].MouseInput.Y = (positionY*65535)/Screen.PrimaryScreen.Bounds.Height;
            i[0].MouseInput.Flags = NativeMethods.MouseEventAbsolute | NativeMethods.MouseEventMove;

            // send it off
            uint result = NativeMethods.SendInput(1, i, Marshal.SizeOf(i[0]));
            if (result == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public static void MoveMouseRelative(int positionX, int positionY)
        {
            Input[] i = new Input[1];

            // move the mouse to the position specified
            i[0] = new Input();
            i[0].Type = NativeMethods.InputMouse;
            i[0].MouseInput.X = positionX;
            i[0].MouseInput.Y = positionY;
            i[0].MouseInput.Flags = NativeMethods.MouseEventMove;

            // send it off
            uint result = NativeMethods.SendInput(1, i, Marshal.SizeOf(i[0]));
            if (result == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public static void ClickEvent(bool left, bool up)
        {
            Input[] i = new Input[1];

            // move the mouse to the position specified
            i[0] = new Input();
            i[0].Type = NativeMethods.InputMouse;
            if (left)
            {
                if (up)
                {
                    i[0].MouseInput.Flags = NativeMethods.MouseEventLeftUp;
                }
                else
                {
                    i[0].MouseInput.Flags = NativeMethods.MouseEventLeftDown;
                }
            }
            else
            {
                if (up)
                {
                    i[0].MouseInput.Flags = NativeMethods.MouseEventRightUp;
                }
                else
                {
                    i[0].MouseInput.Flags = NativeMethods.MouseEventRightDown;
                }
            }
            // send it off
            uint result = NativeMethods.SendInput(1, i, Marshal.SizeOf(i[0]));
            if (result == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public static void WheelEvent(bool up)
        {
            Input[] i = new Input[1];

            // move the mouse to the position specified
            i[0] = new Input();
            i[0].Type = NativeMethods.InputMouse;
            i[0].MouseInput.Flags = NativeMethods.MouseeventfWheel;
            if (up)
            {
                i[0].MouseInput.MouseData = NativeMethods.WheelUp;
            }
            else
            {
                i[0].MouseInput.MouseData = NativeMethods.WheelDown;
            }
            // send it off
            uint result = NativeMethods.SendInput(1, i, Marshal.SizeOf(i[0]));
            if (result == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            NativeMethods.GetCursorPos(out lpPoint);
            //bool success = User32.GetCursorPos(out lpPoint);
            // if (!success)

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
