using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using HSR.PresWriter.PenTracking;
using HSR.PresWriter.PenTracking.Events;
using Parser;
using Parser.Mock;

namespace ImageChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //MockCameraConnector cameraConnector = new MockCameraConnector();
            var parser = new DataParser(); // TODO
            parser.Start();
            //cameraConnector.NewImage += delegate(object sender, NewImageEventArgs args)
                {
                    MemoryStream ms = new MemoryStream();
                    //args.NewImage.GetVisual().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();
                    Action a = delegate { Img.Source = bi; };
                    Img.Dispatcher.Invoke(a);
                };
        }
    }
}
