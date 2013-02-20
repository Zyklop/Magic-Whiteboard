using System;

namespace Parser.Events
{
    public class NewImageEventArgs:EventArgs
    {
        public Bitmap NewImage { get; set; } 
    }
}
