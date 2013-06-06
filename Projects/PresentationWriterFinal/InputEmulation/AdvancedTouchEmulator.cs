using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HSR.PresWriter.Common;

namespace InputEmulation
{
    internal class AdvancedTouchEmulator : IAdvancedInputEmulator
    {
        private long _lastContact;
        private readonly Touch _touch;
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private bool _touchdown;
        private Queue<System.Drawing.Point> _cache;

        public AdvancedTouchEmulator(int touchPoints, int screenWidth, int screenHeight)
        {
            _cache = new Queue<System.Drawing.Point>();
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _touch = new Touch(touchPoints, Touch.FeedbackMode.DEFAULT);
        }

        public int RightClickTimeOut { get { return 1000; }
            set { }
        }
        public int ReleaseTimeout { get; set; }
        public int KeyboardReleaseTimeout { get; set; }
        public int Radius { get; set; }
        public int BorderWidth { get; set; }
        public int AverageCount { get; set; }
        public int TouchPoints { get; set; }

        public void NoData()
        {
            if (CurrentMillis.Millis - _lastContact > ReleaseTimeout && _touchdown) // timed out, not waiting for a new contact
                _touch.Release();
            else
                _touch.Hold();
        }

        public void NewPoint(System.Drawing.Point p)
        {
            NoData(); // check timeout and optionally release buttons
            if (p.X < 0 && p.X > -1*BorderWidth)
            {
                // left from screen
            }
            else if (p.X > _screenWidth && p.X < _screenWidth + BorderWidth)
            {
                // right from screen
            }
            else if (p.Y > _screenHeight && p.Y < _screenHeight + BorderWidth)
            {
                // under the screen
                if (ShowMenu != null)
                    ShowMenu(this, null);
            }
            else if (p.Y < 0 && p.Y > -1*BorderWidth)
            {
                // over the screen
            }
            else if (p.X >= 0 && p.X <= _screenWidth && p.Y >= 0 && p.Y <= _screenHeight)
            {
                _cache.Enqueue(p);
                while (_cache.Count >= AverageCount)
                {
                    _cache.Dequeue();
                }
                if (!_touchdown)
                {
                    _touch.Touchdown(Average.X, Average.Y);
                    _touchdown = true;
                }
                else
                    _touch.DragTo(Average.X, Average.Y);
            }
        }

        public event EventHandler ShowMenu;


        private System.Drawing.Point Average
        {
            get
            {
                return new System.Drawing.Point((int) Math.Round(_cache.Average(p => p.X)),
                                        (int) Math.Round(_cache.Average(p => p.Y)));
            }
        }
    }
}
