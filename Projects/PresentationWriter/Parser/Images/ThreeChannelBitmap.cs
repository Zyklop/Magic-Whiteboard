
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace HSR.PresentationWriter.Parser.Images
{
    public class ThreeChannelBitmap
    {
        private OneChannelBitmap _r;
        private OneChannelBitmap _g;
        private OneChannelBitmap _b;

        public ThreeChannelBitmap()
        {
            _r = new OneChannelBitmap();
            _g = new OneChannelBitmap();
            _b = new OneChannelBitmap();
        }

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

        public static async Task<ThreeChannelBitmap> FromBitmapAsync(Image bitmap)
        {
            var res = new ThreeChannelBitmap(bitmap.Width, bitmap.Height);
            var bm = new Bitmap(bitmap);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var c = bm.GetPixel(i,j);
                    res._r.Channel[i, j] = c.R;
                    res._g.Channel[i, j] = c.G;
                    res._b.Channel[i, j] = c.B;
                }
            }
            return res;
        }

        public byte[,] R { get { return _r.Channel; } set { _r.Channel = value; } }

        public byte[,] G { get { return _g.Channel; } set { _g.Channel = value; } }

        public byte[,] B { get { return _b.Channel; } set { _b.Channel = value; } }

        public OneChannelBitmap RChannelBitmap { get { return _r; } set { _r = value; } }

        public OneChannelBitmap GChannelBitmap { get { return _g; } set { _g = value; } }

        public OneChannelBitmap BChannelBitmap { get { return _b; } set { _b = value; } }

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
