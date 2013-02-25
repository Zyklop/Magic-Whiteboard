using System;
using System.Drawing;

namespace Parser.Images
{
    public struct ThreeChannelBitmap
    {
        private OneChannelBitmap _r;
        private OneChannelBitmap _g;
        private OneChannelBitmap _b;

        public ThreeChannelBitmap(int width, int height)
        {
            _r = new OneChannelBitmap(width, height);
            _g = new OneChannelBitmap(width, height);
            _b = new OneChannelBitmap(width, height);
        }

        public ThreeChannelBitmap(byte[,] r, byte[,] g, byte[,] b)
        {
            _r = new OneChannelBitmap(r);
            _g = new OneChannelBitmap(g);
            _b = new OneChannelBitmap(b);
        }

        public ThreeChannelBitmap(OneChannelBitmap r, OneChannelBitmap g, OneChannelBitmap b)
        {
            _r = r;
            _g = g;
            _b = b;
        }

        public ThreeChannelBitmap(Bitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            _r = new OneChannelBitmap(width, height);
            _g = new OneChannelBitmap(width, height);
            _b = new OneChannelBitmap(width, height);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var c = bitmap.GetPixel(i,j);
                    _r.Channel[i, j] = c.R;
                    _g.Channel[i, j] = c.G;
                    _b.Channel[i, j] = c.B;
                }
            }
        }

        public byte[,] R { get { return _r.Channel; } set { _r.Channel = value; } }

        public byte[,] G { get { return _g.Channel; } set { _g.Channel = value; } }

        public byte[,] B { get { return _b.Channel; } set { _b.Channel = value; } }

        public int Width {
            get { return _r.Channel.GetLength(0); }
        }

        public int Height { get { return _r.Channel.GetLength(1); } }

        public OneChannelBitmap GetGrayscale()
        {
            var res = new OneChannelBitmap(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    res.Channel[i, j] = Avg(_r.Channel[i, j], _g.Channel[i, j], _r.Channel[i,j]);
                }
            }
            return res;
        }

        public Image GetVisual()
        {
            var res = new Bitmap(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    var c = Color.FromArgb(255, _r.Channel[i, j], _g.Channel[i, j], _b.Channel[i, j]);
                    res.SetPixel(i,j,c);
                }
            }
            return res;
        }

        public static ThreeChannelBitmap operator +(ThreeChannelBitmap a1, ThreeChannelBitmap a2)
        {
            return new ThreeChannelBitmap(a1._r + a2._r, a1._g + a2._g, a1._b + a2._b);
        }

        public static ThreeChannelBitmap operator -(ThreeChannelBitmap a1, ThreeChannelBitmap a2)
        {

            return new ThreeChannelBitmap(a1._r - a2._r, a1._g - a2._g, a1._b - a2._b);
        }

        private static byte Diff(byte b1, byte b2)
        {
            if (b1 > b2) return (byte) (b1 - b2);
            return (byte) (b2 - b1);
        }

        private static byte Sum(byte b1, byte b2)
        {
            var r = b1 + b2;
            return (byte) (r > 255 ? 255 : r);
        }

        private static byte Avg(byte b1, byte b2, byte b3)
        {
            return (byte) (Math.Round(((b1 + b2 + b3) / 3.0)));
        }
    }
}
