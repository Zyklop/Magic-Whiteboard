using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Windows.Forms;
using HSR.PresWriter.Common;

namespace InputEmulation
{
    internal class AdvancedMouseEmulator : IAdvancedInputEmulator
    {
        private System.Drawing.Point _start;
        private System.Drawing.Point _lastPosition = new System.Drawing.Point(-1,-1);
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private Queue<System.Drawing.Point> _cache;
        private long _startTime;
        private long _lastContact;
        private long _lastKeyPressed;
        private bool _waiting;
        private bool _leftCicked;
        private bool _rightCicked;

        public AdvancedMouseEmulator(int screenWidth, int screenHeight)
        {
            _cache = new Queue<System.Drawing.Point>();
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            RightClickTimeOut = 1000;
            ReleaseTimeout = 160;
            KeyboardReleaseTimeout = 500;
            Radius = 5;
            BorderWidth = 200;
        }

        public int RightClickTimeOut { get; set; }

        public int ReleaseTimeout { get; set; }

        public int KeyboardReleaseTimeout { get; set; }

        public int Radius { get; set; }

        public int BorderWidth { get; set; }

        // no contact detected
        public void NoData()
        {
            if (CurrentMillis.Millis - _lastContact > ReleaseTimeout) // timed out, not waiting for a new contact
            {
                //cleanup
                if (_waiting)
                {
                    // left click
                    _waiting = false;
                    Mouse.ClickEvent(true, false);
                    Thread.Sleep(7);
                    Mouse.ClickEvent(true, true);
                }
                if (_leftCicked)
                {
                    //release left mouse button
                    _leftCicked = false;
                    _cache.Clear();
                    Mouse.ClickEvent(true, true);
                }
                if (_rightCicked)
                {
                    // release right mouse button
                    _rightCicked = false;
                    _cache.Clear();
                    Mouse.ClickEvent(false, true);
                }
            }
            // wait
        }

        /// <summary>
        /// A new contact has been detected
        /// </summary>
        /// <param name="p"></param>
        public void NewPoint(System.Drawing.Point p)
        {
            NoData(); // check timeout and optionally release buttons
            if (p.X < 0 && p.X > -1*BorderWidth)
            {
                // left from screen
                if (!_leftCicked && !_rightCicked && CurrentMillis.Millis - _lastKeyPressed > KeyboardReleaseTimeout)
                {
                    VirtualKeys.SendKeyAsInput(Keys.P, 7);
                    _lastKeyPressed = CurrentMillis.Millis;
                }
            }
            else if (p.X > _screenWidth && p.X < _screenWidth + BorderWidth)
            {
                // right from screen
                if (!_leftCicked && !_rightCicked && CurrentMillis.Millis - _lastKeyPressed > KeyboardReleaseTimeout)
                {
                    VirtualKeys.SendKeyAsInput(Keys.N, 7);
                    _lastKeyPressed = CurrentMillis.Millis;
                }
            }
            else if (p.Y > _screenHeight && p.Y < _screenHeight + BorderWidth)
            {
                // under the screen
                if (ShowMenu != null)
                    ShowMenu(this, null);
            }
            else if (p.Y < 0 && p.Y > -1 * BorderWidth)
            {
                // over the screen
            }
            else if (p.X >= 0 && p.X <= _screenWidth && p.Y >= 0 && p.Y <= _screenHeight)
            {
                if (_lastPosition.X == -1)
                {
                    //set starting Position
                    _lastPosition = p;
                }
                _cache.Enqueue(p);
                while (_cache.Count >= AverageCount)
                {
                    _cache.Dequeue();
                }
                if (!_leftCicked && !_rightCicked)
                    Mouse.MoveMouseAbsolute(p.X, p.Y);
                else
                    MoveToAverage();
                if (!_waiting && !_leftCicked && !_rightCicked)
                {
                    // no recent contact
                    Mouse.MoveMouseAbsolute(p.X, p.Y);
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
                        Mouse.MoveMouseAbsolute(_start.X, _start.Y);
                        Thread.Sleep(1);
                        Mouse.ClickEvent(false, false);
                        MoveToAverage();
                        _waiting = false;
                    }
                    else if (Math.Abs(_start.X - p.X) > Radius || Math.Abs(_start.Y - p.Y) > Radius)
                    {
                        // moved outside the defined radius
                        _waiting = false;
                        Mouse.MoveMouseAbsolute(_start.X, _start.Y);
                        Thread.Sleep(1);
                        Mouse.ClickEvent(true, false);
                        MoveToAverage();
                        _leftCicked = true;
                    }
                }
                Debug.WriteLine("Wait time: " + (CurrentMillis.Millis - _startTime) + (_waiting ? "Waiting, " : "") +
                                (_leftCicked ? "left down, " : "leftUp, ") + (_rightCicked ? "right down" : "rightUp, "));
                _lastContact = CurrentMillis.Millis;
                _lastPosition = p;
            }
        }

        public event EventHandler ShowMenu;

        private void MoveToAverage()
        {
            Mouse.MoveMouseAbsolute((int)Math.Round(_cache.Average(p => p.X)),
                    (int)Math.Round(_cache.Average(p => p.Y)));
        }

        public int AverageCount { get; set; }
    }
}
