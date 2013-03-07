using System.Drawing;
using System.Threading;
using System.Windows;
using Point = System.Windows.Point;

namespace WFVisuslizer
{
    public class VisualizerControl
    {
        private Form1 _cw = new Form1();

        public VisualizerControl()
        {
            
        }

        public bool Transparent { get { return _cw.Transparent; } set { _cw.Transparent = value; } }

        public void AddRect(Point topLeft, Point bottomRight, Color fromRgb)
        {
            _cw.AddRect(topLeft, bottomRight, fromRgb);
        }

        public void AddRect(Rect rect, Color c)
        {
            AddRect(new Point((int) rect.TopLeft.X, (int) rect.TopLeft.Y), new Point((int) rect.BottomRight.X, (int) rect.BottomRight.Y), c);
        }

        public void ClearRects()
        {
            _cw.ClearRects();
        }

        public void Close()
        {
            _cw.Close();
        }

        public void Show()
        {
            _cw.Show();
        }

        public int Width { get
        {
            //int i = -1;
            //_cw.Dispatcher.Invoke(() => i = (int) _cw.ActualWidth);
            //Debug.WriteLine(i);
            //return i;
            return (int) _cw.Width;
        } }

        public int Height { get { return (int) _cw.Height; } }

        public void AddRect(int topLeft, int bottomRight, int width, int height, Color color)
        {
            _cw.AddRect(topLeft, bottomRight, width, height, color);
        }
    }
}
