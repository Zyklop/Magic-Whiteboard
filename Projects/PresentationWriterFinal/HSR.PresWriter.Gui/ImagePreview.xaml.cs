using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PresWriter.Common.IO.Events;

namespace HSR.PresWriter.Gui
{
    /// <summary>
    /// Interaction logic for ImagePreview.xaml
    /// </summary>
    public partial class ImagePreview : Window
    {
        public ImagePreview()
        {
            InitializeComponent();
        }

        internal void NewImg(object sender, FrameReadyEventArgs e)
        {
            if (Dispatcher.CheckAccess())
                CamImg.Source = Tools.CopyBitmap(e.Frame.Bitmap);
            else
                Dispatcher.Invoke(() => CamImg.Source = Tools.LoadBitmap(e.Frame.Bitmap));
        }
    }
}
