using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visualizer
{
    public class VisualizerDummy:IVisualizerControl
    {
        public bool Transparent { get; set; }
        public int Width { get { return 1280; } }
        public int Height { get { return 1024; } }
        public void AddRect(Point topLeft, Point bottomRight, Color fromRgb)
        {
        }

        public void Clear()
        {
        }

        public void Close()
        {
        }

        public void Show()
        {
        }

        public void Draw()
        {
        }

        public void AddRect(int topLeft, int bottomRight, int width, int height, Color color)
        {
        }
    }
}
