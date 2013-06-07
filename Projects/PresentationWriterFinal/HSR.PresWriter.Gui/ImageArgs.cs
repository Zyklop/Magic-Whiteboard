using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HSR.PresWriter.Gui
{
    public class ImageArgs:EventArgs
    {
        public BitmapSource Image { get; set; }
    }
}
