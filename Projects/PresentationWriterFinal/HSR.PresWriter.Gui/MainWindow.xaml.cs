using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
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
using HSR.PresWriter.DataSources;
using HSR.PresWriter.DataSources.Cameras;
using PresWriter.Common.IO.Events;
using System.Configuration;

namespace HSR.PresWriter.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<KeyValuePair<string, string>> _cams;
        private AForgeCamera _camera;

        public MainWindow()
        {
            InitializeComponent();
            Left = (SystemParameters.FullPrimaryScreenWidth - 800)/2;
            Top = SystemParameters.FullPrimaryScreenHeight - 200;
            AllowsTransparency = true;
            Background = new SolidColorBrush(Color.FromArgb(64,0,0,0));
            _cams = AForgeCamera.GetCameras().ToList();
            foreach (var cam in _cams)
            {
                CameraBox.Items.Add(cam.Value);
            }
            CameraBox.SelectionChanged += CameraSelected;
            CameraBox.SelectedIndex = Properties.Settings.Default.Camera;
        }

        private void CameraSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_camera != null)
                _camera.FrameReady -= NewImage;
            Properties.Settings.Default.Camera = CameraBox.SelectedIndex;
            Properties.Settings.Default.Save();
            _camera = new AForgeCamera(_cams[CameraBox.SelectedIndex].Key);
            _camera.FrameReady += NewImage;
            _camera.Start();
        }

        private void NewImage(object sender, FrameReadyEventArgs e)
        {
            if (Dispatcher.CheckAccess())
                CameraImage.Source = Tools.CopyBitmap(e.Frame.Bitmap);
            else
                Dispatcher.Invoke(() => CameraImage.Source = Tools.LoadBitmap(e.Frame.Bitmap));
        }

        private void ShowImage(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ShowConfig(object sender, RoutedEventArgs e)
        {
            if(_camera != null)
                _camera.ShowConfigurationDialog();
        }

        private void MirrorCam(object sender, RoutedEventArgs e)
        {
            if (_camera != null && MirrorBtn.IsChecked.HasValue)
                _camera.IsMirrored = MirrorBtn.IsChecked.Value;
        }
    }
}
