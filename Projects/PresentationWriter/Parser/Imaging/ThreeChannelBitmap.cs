
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HSR.PresentationWriter.Parser.Images
{
    public class ThreeChannelBitmap
    {
        private OneChannelBitmap _r;
        private OneChannelBitmap _g;
        private OneChannelBitmap _b;

        /// <summary>
        /// Create a new bitmap with size 0,0 
        /// </summary>
        public ThreeChannelBitmap()
        {
            _r = new OneChannelBitmap();
            _g = new OneChannelBitmap();
            _b = new OneChannelBitmap();
        }

        /// <summary>
        /// Creates an empty bitmap with the desired size
        /// </summary>
        /// <param name="width">desired width</param>
        /// <param name="height">desired height</param>
        public ThreeChannelBitmap(int width, int height)
        {
            _r = new OneChannelBitmap(width, height);
            _g = new OneChannelBitmap(width, height);
            _b = new OneChannelBitmap(width, height);
        }

        /// <summary>
        /// Create a bitmap from an byte array
        /// </summary>
        /// <param name="r">Red source</param>
        /// <param name="g">Green source</param>
        /// <param name="b">Blue source</param>
        public ThreeChannelBitmap(byte[,] r, byte[,] g, byte[,] b)
        {
            _r = new OneChannelBitmap(r);
            _g = new OneChannelBitmap(g);
            _b = new OneChannelBitmap(b);
        }

        /// <summary>
        /// Create a bitmap from Channels
        /// </summary>
        /// <param name="r">Red source</param>
        /// <param name="g">Green source</param>
        /// <param name="b">Blue source</param>
        public ThreeChannelBitmap(OneChannelBitmap r, OneChannelBitmap g, OneChannelBitmap b)
        {
            _r = r;
            _g = g;
            _b = b;
        }

        /// <summary>
        /// Create from a System image
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static async Task<ThreeChannelBitmap> FromBitmapAsync(Image bitmap)
        {
            var res = new ThreeChannelBitmap(bitmap.Width, bitmap.Height);
            //var bm = new Bitmap(bitmap);
            //for (int i = 0; i < bitmap.Width; i++)
            //{
            //    for (int j = 0; j < bitmap.Height; j++)
            //    {
            //        var c = bm.GetPixel(i,j);
            //        res._r.Channel[i, j] = c.R;
            //        res._g.Channel[i, j] = c.G;
            //        res._b.Channel[i, j] = c.B;
            //    }
            //}
            //return res;

            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = ((Bitmap)bitmap).LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride*bitmap.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            int stride = bmpData.Stride;

            for (int column = 0; column < bmpData.Height; column++)
            {
                for (int row = 0; row < bmpData.Width; row++)
                {
                    res.B[column, row] = rgbValues[(column * stride) + (row * 3)];
                    res.G[column, row] = rgbValues[(column * stride) + (row * 3) + 1];
                    res.R[column, row] = rgbValues[(column * stride) + (row * 3) + 2];
                }
            }
            return res;
        }

        /// <summary>
        /// Red Channel
        /// </summary>
        public byte[,] R { get { return _r.Channel; } set { _r.Channel = value; } }

        /// <summary>
        /// Green Channel
        /// </summary>
        public byte[,] G { get { return _g.Channel; } set { _g.Channel = value; } }

        /// <summary>
        /// Blue Channel
        /// </summary>
        public byte[,] B { get { return _b.Channel; } set { _b.Channel = value; } }

        /// <summary>
        /// Red Bitmap
        /// </summary>
        public OneChannelBitmap RChannelBitmap { get { return _r; } set { _r = value; } }

        /// <summary>
        /// Green Bitmap
        /// </summary>
        public OneChannelBitmap GChannelBitmap { get { return _g; } set { _g = value; } }

        /// <summary>
        /// Blue Bitmap
        /// </summary>
        public OneChannelBitmap BChannelBitmap { get { return _b; } set { _b = value; } }

        /// <summary>
        /// Image width
        /// </summary>
        public int Width {
            get { return _r.Channel.GetLength(0); }
        }
    
        /// <summary>
        /// Image height
        /// </summary>
        public int Height { get { return _r.Channel.GetLength(1); } }

        /// <summary>
        /// Transform to a grayscale image
        /// by building the average of all channels
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Transform to a System Bitmap
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Add all channels seperate
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        public static ThreeChannelBitmap operator +(ThreeChannelBitmap a1, ThreeChannelBitmap a2)
        {
            return new ThreeChannelBitmap(a1._r + a2._r, a1._g + a2._g, a1._b + a2._b);
        }

        /// <summary>
        /// Building a difference bitmap of all seperate channels.
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Transform from a System image synchronous
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static ThreeChannelBitmap FromBitmap(Bitmap bitmap)
        {
            var res = new ThreeChannelBitmap(bitmap.Width, bitmap.Height);
            //var bm = new Bitmap(bitmap);
            //for (int i = 0; i < bitmap.Width; i++)
            //{
            //    for (int j = 0; j < bitmap.Height; j++)
            //    {
            //        var c = bm.GetPixel(i, j);
            //        res._r.Channel[i, j] = c.R;
            //        res._g.Channel[i, j] = c.G;
            //        res._b.Channel[i, j] = c.B;
            //    }
            //}

            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = ((Bitmap)bitmap).LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bitmap.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            int stride = bmpData.Stride;

            for (int column = 0; column < bmpData.Height; column++)
            {
                for (int row = 0; row < bmpData.Width; row++)
                {
                    res.B[row, column] = rgbValues[(column * stride) + (row * 3)];
                    res.G[row, column] = rgbValues[(column * stride) + (row * 3) + 1];
                    res.R[row, column] = rgbValues[(column * stride) + (row * 3) + 2];
                }
            }
            return res;
        }
    }
}
