using System;

namespace HSR.PresentationWriter.Parser.Images
{
    public struct OneChannelBitmap
    {
        private byte[,] _c;

        public OneChannelBitmap(int width, int height)
        {
            _c = new byte[width, height];
        }

        public OneChannelBitmap(byte[,] src)
        {
            _c = src;
        }

        public byte[,] Channel
        {
            get { return _c; }
            set
            {
            if (value.Length == _c.Length && value.GetLength(1)==_c.GetLength(1))
            {
                _c = value;
            }
            else
            {
                throw new ArgumentException("Size missmatch");
            }
        } }

        public int Width
        {
            get { return _c.GetLength(0); }
        }

        public int Height { get { return _c.GetLength(1); } }

        public static OneChannelBitmap operator +(OneChannelBitmap a1, OneChannelBitmap a2)
        {
            var width = a1.Width;
            var height = a1.Height;
            var res = new OneChannelBitmap(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    res._c[i, j] = Sum(a1._c[i, j], a2._c[i, j]);
                }
            }
            return res;
        }

        public static OneChannelBitmap operator -(OneChannelBitmap a1, OneChannelBitmap a2)
        {
            var width = a1.Width;
            var height = a1.Height;
            var res = new OneChannelBitmap(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    res._c[i, j] = Diff(a1._c[i, j], a2._c[i, j]);
                }
            }
            return res;
        }

        private static byte Diff(byte b1, byte b2)
        {
            if (b1 > b2) return (byte)(b1 - b2);
            return (byte)(b2 - b1);
        }

        private static byte Sum(byte b1, byte b2)
        {
            var r = b1 + b2;
            return (byte)(r > 255 ? 255 : r);
        }
    }
}
