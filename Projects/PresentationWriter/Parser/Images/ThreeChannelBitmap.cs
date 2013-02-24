namespace HSR.PresentationWriter.Parser.Images
{
    internal struct ThreeChannelBitmap
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

        public ThreeChannelBitmap(char[,] r, char[,] g, char[,] b)
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

        public char[,] R { get { return _r.Channel; } set { _r.Channel = value; } }

        public char[,] G { get { return _g.Channel; } set { _g.Channel = value; } }

        public char[,] B { get { return _b.Channel; } set { _b.Channel = value; } }

    }
}
