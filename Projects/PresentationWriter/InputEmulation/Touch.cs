using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InputEmulation
{
    public class Touch
    {
        public enum FeedbackMode
        {
            /// <summary>
            /// Specifies default touch visualizations.
            /// </summary>
            DEFAULT = 0x1,

            /// <summary>
            /// Specifies indirect touch visualizations.
            /// </summary>
            INDIRECT = 0x2,

            /// <summary>
            /// Specifies no touch visualizations.
            /// </summary>
            NONE = 0x3
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct RECT
        {
            [FieldOffset(0)]
            public int left;
            [FieldOffset(4)]
            public int top;
            [FieldOffset(8)]
            public int right;
            [FieldOffset(12)]
            public int bottom;
        }


        [StructLayout(LayoutKind.Sequential)]
        internal struct POINTER_INFO
        {
            public uint pointerType;
            public uint pointerId;
            public uint frameId;
            public uint pointerFlags;
            public IntPtr sourceDevice;
            public IntPtr hwndTarget;
            public Mouse.POINT ptPixelLocation;
            public Mouse.POINT ptHimetricLocation;
            public Mouse.POINT ptPixelLocationRaw;
            public Mouse.POINT ptHimetricLocationRaw;
            public uint dwTime;
            public uint historyCount;
            public uint inputData;
            public uint dwKeyStates;
            public ulong PerformanceCount;
            public uint ButtonChangeType;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINTER_TOUCH_INFO
        {
            public POINTER_INFO pointerInfo;
            public uint touchFlags;
            public uint touchMask;
            public RECT rcContact;
            public RECT rcContactRaw;
            public uint orientation;
            public uint pressure;
        }

        internal static class TouchApi
        {
            public const int POINTER_FLAG_NONE = 0x00000000;
            public const int POINTER_FLAG_NEW = 0x00000001;
            public const int POINTER_FLAG_INRANGE = 0x00000002;
            public const int POINTER_FLAG_INCONTACT = 0x00000004;
            public const int POINTER_FLAG_FIRSTBUTTON = 0x00000010;
            public const int POINTER_FLAG_SECONDBUTTON = 0x00000020;
            public const int POINTER_FLAG_THIRDBUTTON = 0x00000040;
            public const int POINTER_FLAG_OTHERBUTTON = 0x00000080;
            public const int POINTER_FLAG_PRIMARY = 0x00000100;
            public const int POINTER_FLAG_CONFIDENCE = 0x00000200;
            public const int POINTER_FLAG_CANCELLED = 0x00000400;
            public const int POINTER_FLAG_DOWN = 0x00010000;
            public const int POINTER_FLAG_UPDATE = 0x00020000;
            public const int POINTER_FLAG_UP = 0x00040000;
            public const int POINTER_FLAG_WHEEL = 0x00080000;
            public const int POINTER_FLAG_HWHEEL = 0x00100000;

            public const int PT_POINTER = 0x00000001;
            public const int PT_TOUCH = 0x00000002;
            public const int PT_PEN = 0x00000003;
            public const long PT_MOUSE = 0x00000004;

            public const int TOUCH_MASK_NONE = 0x00000000; // Default. None of the optional fields are valid.
            public const int TOUCH_MASK_CONTACTAREA = 0x00000001; // rcContact of the POINTER_TOUCH_INFO structure is valid.
            public const int TOUCH_MASK_ORIENTATION = 0x00000002; // orientation of the POINTER_TOUCH_INFO structure is valid.
            public const int TOUCH_MASK_PRESSURE =  0x00000004; //pressure of the POINTER_TOUCH_INFO structure is valid.

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern bool InitializeTouchInjection(uint maxCount, uint dwMode);

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern bool InjectTouchInput(uint count, [MarshalAs(UnmanagedType.LPArray), In]POINTER_TOUCH_INFO[] contacts);
        }

        public Touch(uint touchPoints, FeedbackMode mode)
        {
            if(!TouchApi.InitializeTouchInjection(touchPoints, (uint) mode))
                throw new ExternalException("Initialisation failed. Code: " + Marshal.GetLastWin32Error());
        }

        public void Touchdown(int x, int y)
        {
            var contact = new POINTER_TOUCH_INFO();

            contact.pointerInfo.pointerType = TouchApi.PT_TOUCH;
            contact.pointerInfo.pointerId = 0;          //contact 0
            contact.pointerInfo.ptPixelLocation.X = x; // Y co-ordinate of touch on screen
            contact.pointerInfo.ptPixelLocation.Y = y; // X co-ordinate of touch on screen

            contact.touchFlags = TouchApi.POINTER_FLAG_NONE;
            contact.touchMask = TouchApi.TOUCH_MASK_CONTACTAREA | TouchApi.TOUCH_MASK_ORIENTATION | TouchApi.TOUCH_MASK_PRESSURE;
            contact.orientation = 90; // Orientation of 90 means touching perpendicular to screen.
            contact.pressure = 32000;
            contact.pointerInfo.pointerFlags = TouchApi.POINTER_FLAG_DOWN | TouchApi.POINTER_FLAG_INRANGE | TouchApi.POINTER_FLAG_INCONTACT;

            // defining contact area (I have taken area of 4 x 4 pixel)
            contact.rcContact.top = contact.pointerInfo.ptPixelLocation.Y - 2;
            contact.rcContact.bottom = contact.pointerInfo.ptPixelLocation.Y + 2;
            contact.rcContact.left = contact.pointerInfo.ptPixelLocation.X - 2;
            contact.rcContact.right = contact.pointerInfo.ptPixelLocation.X + 2;

            POINTER_TOUCH_INFO[] args = new[]{contact};
            if (!TouchApi.InjectTouchInput(1,args))
                throw new ExternalException("Injection failed. Code: " + Marshal.GetLastWin32Error());
        }

        public void Release(int x, int y)
        {
            var contact = new POINTER_TOUCH_INFO();

            contact.pointerInfo.pointerType = TouchApi.PT_TOUCH;
            contact.pointerInfo.pointerId = 0;          //contact 0
            contact.pointerInfo.ptPixelLocation.X = x; // Y co-ordinate of touch on screen
            contact.pointerInfo.ptPixelLocation.Y = y; // X co-ordinate of touch on screen

            contact.touchFlags = TouchApi.POINTER_FLAG_NONE;
            contact.touchMask = TouchApi.TOUCH_MASK_CONTACTAREA | TouchApi.TOUCH_MASK_ORIENTATION | TouchApi.TOUCH_MASK_PRESSURE;
            contact.orientation = 90; // Orientation of 90 means touching perpendicular to screen.
            contact.pressure = 32000;
            contact.pointerInfo.pointerFlags = TouchApi.POINTER_FLAG_UP;

            // defining contact area (I have taken area of 4 x 4 pixel)
            contact.rcContact.top = contact.pointerInfo.ptPixelLocation.Y - 2;
            contact.rcContact.bottom = contact.pointerInfo.ptPixelLocation.Y + 2;
            contact.rcContact.left = contact.pointerInfo.ptPixelLocation.X - 2;
            contact.rcContact.right = contact.pointerInfo.ptPixelLocation.X + 2;

            POINTER_TOUCH_INFO[] args = new[] { contact };
            if (!TouchApi.InjectTouchInput(1, args))
                throw new ExternalException("Initialisation failed. Code: " + Marshal.GetLastWin32Error());
        }

        public void DragTo(int x, int y)
        {
            var contact = new POINTER_TOUCH_INFO();

            contact.pointerInfo.pointerType = TouchApi.PT_TOUCH;
            contact.pointerInfo.pointerId = 0;          //contact 0
            contact.pointerInfo.ptPixelLocation.X = x; // Y co-ordinate of touch on screen
            contact.pointerInfo.ptPixelLocation.Y = y; // X co-ordinate of touch on screen

            contact.touchFlags = TouchApi.POINTER_FLAG_NONE;
            contact.touchMask = TouchApi.TOUCH_MASK_CONTACTAREA | TouchApi.TOUCH_MASK_ORIENTATION | TouchApi.TOUCH_MASK_PRESSURE;
            contact.orientation = 90; // Orientation of 90 means touching perpendicular to screen.
            contact.pressure = 32000;
            contact.pointerInfo.pointerFlags = TouchApi.POINTER_FLAG_UPDATE | TouchApi.POINTER_FLAG_INRANGE | TouchApi.POINTER_FLAG_INCONTACT;

            // defining contact area (I have taken area of 4 x 4 pixel)
            contact.rcContact.top = contact.pointerInfo.ptPixelLocation.Y - 2;
            contact.rcContact.bottom = contact.pointerInfo.ptPixelLocation.Y + 2;
            contact.rcContact.left = contact.pointerInfo.ptPixelLocation.X - 2;
            contact.rcContact.right = contact.pointerInfo.ptPixelLocation.X + 2;

            POINTER_TOUCH_INFO[] args = new[] { contact };
            if (!TouchApi.InjectTouchInput(1, args))
                throw new ExternalException("Initialisation failed. Code: " + Marshal.GetLastWin32Error());
        }

        public void Hold(int x, int y)
        {
            var contact = new POINTER_TOUCH_INFO();

            contact.pointerInfo.pointerType = TouchApi.PT_TOUCH;
            contact.pointerInfo.pointerId = 0;          //contact 0
            contact.pointerInfo.ptPixelLocation.X = x; // Y co-ordinate of touch on screen
            contact.pointerInfo.ptPixelLocation.Y = y; // X co-ordinate of touch on screen

            contact.touchFlags = TouchApi.POINTER_FLAG_NONE;
            contact.touchMask = TouchApi.TOUCH_MASK_CONTACTAREA | TouchApi.TOUCH_MASK_ORIENTATION | TouchApi.TOUCH_MASK_PRESSURE;
            contact.orientation = 90; // Orientation of 90 means touching perpendicular to screen.
            contact.pressure = 32000;
            contact.pointerInfo.pointerFlags = TouchApi.POINTER_FLAG_UPDATE | TouchApi.POINTER_FLAG_INRANGE | TouchApi.POINTER_FLAG_INCONTACT;

            // defining contact area (I have taken area of 4 x 4 pixel)
            contact.rcContact.top = contact.pointerInfo.ptPixelLocation.Y - 2;
            contact.rcContact.bottom = contact.pointerInfo.ptPixelLocation.Y + 2;
            contact.rcContact.left = contact.pointerInfo.ptPixelLocation.X - 2;
            contact.rcContact.right = contact.pointerInfo.ptPixelLocation.X + 2;

            POINTER_TOUCH_INFO[] args = new[] { contact };
            if (!TouchApi.InjectTouchInput(1, args))
                throw new ExternalException("Initialisation failed. Code: " + Marshal.GetLastWin32Error());
        }
    }
}
