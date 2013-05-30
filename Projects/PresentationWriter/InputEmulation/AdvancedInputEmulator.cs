using System;
using System.Diagnostics;
using System.Threading;
using HSR.PresWriter;

namespace InputEmulation
{
    public class AdvancedInputEmulator
    {
        private static System.Drawing.Point _start;
        private static System.Drawing.Point _lastPosition = new System.Drawing.Point(-1,-1);
        private const int RightClickTimeOut = 3000;
        private const int ReleaseTimeout = 90;
        private const int Radius = 30;
        private static long _startTime;
        private static long _lastContact;
        private static bool _waiting = false;
        private static bool _leftCicked = false;
        private static bool _rightCicked = false;

        // no contact detected
        public static void NoData()
        {
            if (CurrentMillis.Millis - _lastContact > ReleaseTimeout) // timed out, not waiting for a new contact
            {
                //cleanup
                if (_waiting)
                {
                    // left click
                    _waiting = false;
                    Mouse.ClickEvent(true, false);
                    Thread.Sleep(15);
                    Mouse.ClickEvent(true, true);
                }
                if (_leftCicked)
                {
                    //release left mouse button
                    _leftCicked = false;
                    Mouse.ClickEvent(true, true);
                }
                if (_rightCicked)
                {
                    // release right mouse button
                    _rightCicked = false;
                    Mouse.ClickEvent(false, true);
                }
            }
            // wait
        }

        /// <summary>
        /// A new contact has been detected
        /// </summary>
        /// <param name="p"></param>
        public static void NewPoint(System.Drawing.Point p)
        {
            NoData(); // check timeout and optionally release buttons
            if (_lastPosition.X == -1)
            {
                //set starting Position
                _lastPosition = p;
            }
            Mouse.MoveMouseAbsolute(p.X, p.Y);
            //Mouse.MoveMouseRelative(_lastPosition.X - p.X, _lastPosition.Y - p.Y);
            if (!_waiting && !_leftCicked && !_rightCicked)
            {
                // no recent contact
                _start = p;
                _startTime = CurrentMillis.Millis;
                _waiting = true;
                //Mouse.MoveMouseAbsolute(p.X, p.Y);
            }
            else if (_waiting)
            {
                if (CurrentMillis.Millis - _startTime > RightClickTimeOut)
                {
                    // waited enough
                    _rightCicked = true;
                    Mouse.ClickEvent(false, false);
                    _waiting = false;
                }
                else if (Math.Abs(_start.X - p.X) > Radius || Math.Abs(_start.Y - p.Y) > Radius)
                {
                    // moved outside the defined radius
                    _waiting = false;
                    Mouse.ClickEvent(true, false);
                    _leftCicked = true;
                }
            }
            Debug.WriteLine((_waiting?"Waiting, ":"") + (_leftCicked?"left down, ":"") + (_rightCicked?"right down":""));
            _lastContact = CurrentMillis.Millis;
            _lastPosition = p;
        }
    }
}
