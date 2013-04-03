using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Visualizer;
using Point = Visualizer.Point;
using DPoint = System.Windows.Point;

namespace WFVisuslizer
{
    public class VisualizerControl : IVisualizerControl
    {
        private Form1 _cw = new Form1();
        private bool _shown = false;
        private static VisualizerControl _singleton;

        public static VisualizerControl GetVisualizer()
        {
            if (_singleton == null)
            {
                _singleton = new VisualizerControl();
            }
            return _singleton;
        }

        protected VisualizerControl()
        {
            Task.Factory.StartNew(() => Application.Run(_cw));
            _cw.Hide();
            //var thread2 = new Thread(() => Application.Run(_cw));
            //_cw.Hide();
        }

        /// <summary>
        /// Set a transparent background
        /// </summary>
        public bool Transparent { get { return _cw.Transparent; } set { _cw.Transparent = value; } }

        /// <summary>
        /// Show a rectangle
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="fromRgb"></param>
        public void AddRect(Point topLeft, Point bottomRight, Color fromRgb)
        {
            _cw.AddRect(new DPoint(topLeft.X,topLeft.Y), new DPoint(bottomRight.X, bottomRight.Y), fromRgb);
        }

        /// <summary>
        /// Show a rectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="c"></param>
        //public void AddRect(Rect rect, Color c)
        //{
        //    AddRect(new Point((int)rect.TopLeft.X, (int)rect.TopLeft.Y), new Point((int)rect.BottomRight.X, (int)rect.BottomRight.Y), c);
        //}

        /// <summary>
        /// Remove all the rectangles
        /// </summary>
        public void ClearRects()
        {
            _cw.ClearRects();
            //rects.Clear();
        }

        /// <summary>
        /// Close the window when shown
        /// </summary>
        public void Close()
        {
            if (_shown)
            {
                _cw.Hide();
                _shown = false;
                Application.DoEvents();
            }
        }

        /// <summary>
        /// Showing the window if not already open
        /// </summary>
        public void Show()
        {
            if (!_shown)
            {
                _cw.Show();
                _shown = true;
                Application.DoEvents();
            }
        }

        /// <summary>
        /// Width of the shown window
        /// </summary>
        public int Width { get
        {
            //int i = -1;
            //_cw.Dispatcher.Invoke(() => i = (int) _cw.ActualWidth);
            //Debug.WriteLine(i);
            //return i;
            return (int) Screen.PrimaryScreen.Bounds.Width;
        } }

        /// <summary>
        /// Height of the window when shown
        /// </summary>
        public int Height { get { return (int) Screen.PrimaryScreen.Bounds.Height; } }

        /// <summary>
        /// Show a rectangle
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public void AddRect(int topLeft, int bottomRight, int width, int height, Color color)
        {
            _cw.AddRect(topLeft, bottomRight, width, height, color);
        }

        //public void Draw()
        //{
        //    throw new System.NotImplementedException();
        //}
    }
}
