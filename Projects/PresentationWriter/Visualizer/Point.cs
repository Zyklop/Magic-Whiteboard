using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visualizer
{
    public struct Point
    {
        public Point(int x, int y) : this()
        {
            X = x;
            Y = y;
        }

        public Point(double x, double y)
            : this()
        {
            X = (int) x;
            Y = (int) y;
        }

        public int X { get; set; }

        public int Y { get; set; }
    }
}
