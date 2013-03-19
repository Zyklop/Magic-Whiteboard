using System;

namespace HSR.PresentationWriter.Parser.Images
{
    public class OneChannelBitmap
    {
        private byte[,] _c;

        /// <summary>
        /// Create a new bitmap with size 0,0 
        /// </summary>
        public OneChannelBitmap()
        {
            _c = new byte[0,0];
        }

        /// <summary>
        /// Creates an empty bitmap with the desired size
        /// </summary>
        /// <param name="width">desired width</param>
        /// <param name="height">desired height</param>
        public OneChannelBitmap(int width, int height)
        {
            _c = new byte[width, height];
        }

        /// <summary>
        /// Create a bitmap from an byte array
        /// </summary>
        /// <param name="src">image source</param>
        public OneChannelBitmap(byte[,] src)
        {
            _c = src;
        }

        /// <summary>
        /// Values
        /// </summary>
        /// <param name="value">size must match</param>
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

        /// <summary>
        /// Image width
        /// </summary>
        public int Width
        {
            get { return _c.GetLength(0); }
        }

        /// <summary>
        /// Image height
        /// </summary>
        public int Height { get { return _c.GetLength(1); } }

        /// <summary>
        /// Adding two channels
        /// </summary>
        /// <remarks>Overflow protection</remarks>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Difference image
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        /// <remarks>Underflow protected</remarks>
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
