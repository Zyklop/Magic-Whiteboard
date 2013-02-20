using System;

namespace Parser.Images
{
    internal struct OneChannelBitmap
    {
        private char[,] _c;

        public OneChannelBitmap(int width, int height)
        {
            _c = new char[width, height];
        }

        public OneChannelBitmap(char[,] src)
        {
            _c = src;
        }

        public char[,] Channel
        {
            get { return _c; }
            set
            {
            if (value.Length == _c.Length && value.Rank==_c.Rank)
            {
                _c = value;
            }
            else
            {
                throw new ArgumentException("Size missmatch");
            }
        } }
    }
}
