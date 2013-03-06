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
            _cw.Show();
        }

        public static int Width { get { return (int) _cw.Width; } }

        public static int Height { get { return (int) _cw.Height; } }

        public static void AddRect(int topLeft, int bottomRight, int fromRgb, int height, Color color)
        {
            _cw.AddRect(topLeft, bottomRight, fromRgb, height, color);
        }
    }
}
