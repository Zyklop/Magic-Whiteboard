using System;

namespace HSR.PresWriter.PenTracking.Images
{
    public class BinaryBitmap
    {
        private bool[,] _c;

        /// <summary>
        /// Create a new bitmap with size 0,0 
        /// </summary>
        public BinaryBitmap()
        {
            _c = new bool[0,0];
        }

        /// <summary>
        /// Creates an empty bitmap with the desired size
        /// </summary>
        /// <param name="width">desired width</param>
        /// <param name="height">desired height</param>
        public BinaryBitmap(int width, int height)
        {
            _c = new bool[width, height];
        }

        /// <summary>
        /// Create a bitmap from an byte array
        /// </summary>
        /// <param name="src">image source</param>
        public BinaryBitmap(bool[,] src)
        {
            _c = src;
        }

        /// <summary>
        /// Values
        /// </summary>
        /// <param name="value">size must match</param>
        public bool[,] Channel
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
        public BinaryBitmap And(BinaryBitmap a1)
        {
            var width = a1.Width;
            var height = a1.Height;
            var res = new BinaryBitmap(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    res._c[i, j] = this._c[i, j] && a1._c[i, j];
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
        public BinaryBitmap XOR(BinaryBitmap a2)
        {
            var width = a2.Width;
            var height = a2.Height;
            var res = new BinaryBitmap(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    res._c[i, j] = _c[i, j] ^ a2._c[i, j];
                }
            }
            return res;
        }
    }
}
