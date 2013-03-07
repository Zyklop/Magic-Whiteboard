using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ImageVisualizer
{
    public static class VisualizerControl
    {
        private static CalibratorWindow _cw = new CalibratorWindow();

        public static bool Transparent { get { return _cw.Transparent; } set { _cw.Transparent = value; } }

        public static void AddRect(Point topLeft, Point bottomRight, Color fromRgb)
        {
            _cw.AddRect(topLeft, bottomRight, fromRgb);
        }

        public static void AddRect(Rect rect, Color c)
        {
            AddRect(rect.TopLeft, rect.BottomRight, c);
        }

        public static void ClearRects()
        {
            _cw.ClearRects();
        }

        public static void Close()
        {
            _cw.Close();
        }

        public static void Show()
        {
            _cw.Dispatcher.BeginInvoke(new Action(()=> _cw.Show()));
        }

        public static int Width { get
        {
            //int i = -1;
            //_cw.Dispatcher.Invoke(() => i = (int) _cw.ActualWidth);
            //Debug.WriteLine(i);
            //return i;
            return (int) _cw.ActualWidth;
        } }

        public static int Height { get { return (int) _cw.ActualHeight; } }

        public static void AddRect(int topLeft, int bottomRight, int fromRgb, int height, Color color)
        {
            _cw.AddRect(topLeft, bottomRight, fromRgb, height, color);
        }
    }
}
