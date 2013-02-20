using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser
{
    public class Bitmap
    {
        private char[,] _r;
        private char[,] _g;
        private char[,] _b;

        public Bitmap(int height, int width)
        {
            _r = new char[width, height];
            _g = new char[width, height];
            _b = new char[width, height];
        }
    }
}
