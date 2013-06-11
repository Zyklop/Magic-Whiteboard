using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HSR.PresWriter.Common.Containers;
using HSR.PresWriter.DataSources.Cameras;
using HSR.PresWriter.ImageVisualizer;
using HSR.PresWriter.InputEmulation;
using HSR.PresWriter.PenTracking;
using PresWriter.Common.IO.Events;
using Color = System.Windows.Media.Color;
using Pen = System.Drawing.Pen;

namespace HSR.PresWriter.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<KeyValuePair<string, string>> _cams;
        private AForgeCamera _camera;
        private DataParser _parser;
        private IAdvancedInputEmulator _emulator;
        private bool _emulating;
        private bool _shown;

        public MainWindow()
        {
            InitializeComponent();
            Left = (SystemParameters.VirtualScreenWidth - 800)/2;
            Top = SystemParameters.VirtualScreenHeight - 200;
            Topmost = true;
            AllowsTransparency = true;
            Background = new SolidColorBrush(Color.FromArgb(64,0,0,0));
            _cams = AForgeCamera.GetCameras().ToList();
            foreach (var cam in _cams)
            {
                CameraBox.Items.Add(cam.Value);
            }
            CameraBox.SelectionChanged += CameraSelected;
            CameraBox.SelectedIndex = Properties.Settings.Default.Camera;
            MirrorBtn.IsChecked = Properties.Settings.Default.Mirrored;
            _shown = true;
        }

        private void CameraSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_camera != null)
            {
                _camera.FrameReady -= NewImage;
                _camera.Stop();
            }
            Properties.Settings.Default.Camera = CameraBox.SelectedIndex;
            Properties.Settings.Default.Save();
            _camera = new AForgeCamera(_cams[CameraBox.SelectedIndex].Key);
            _camera.FrameReady += NewImage;
            _camera.Start();
        }

        private void NewImage(object sender, FrameReadyEventArgs e)
        {
            var bm = (Bitmap)e.Frame.Bitmap.Clone();
            if(_parser != null && _parser.CalibratorGrid != null && !_parser.CalibratorGrid.TopLeft.IsEmpty)
            {
                using (var g = Graphics.FromImage(bm))
                {
                    g.DrawLine(new Pen(System.Drawing.Color.Red), _parser.CalibratorGrid.TopLeft, _parser.CalibratorGrid.TopRight);
                    g.DrawLine(new Pen(System.Drawing.Color.Red), _parser.CalibratorGrid.TopLeft, _parser.CalibratorGrid.BottomLeft);
                    g.DrawLine(new Pen(System.Drawing.Color.Red), _parser.CalibratorGrid.BottomRight, _parser.CalibratorGrid.TopRight);
                    g.DrawLine(new Pen(System.Drawing.Color.Red), _parser.CalibratorGrid.BottomRight, _parser.CalibratorGrid.BottomLeft);
                    g.Flush();
                }
            }
            if(NewGridFrame != null)
                NewGridFrame(this,new FrameReadyEventArgs(new VideoFrame(0,bm)));
            if (Dispatcher.CheckAccess())
                CameraImage.Source = Tools.CopyBitmap(bm);
            else
                Dispatcher.Invoke(() => CameraImage.Source = Tools.LoadBitmap(bm));
        }

        private void ShowImage(object sender, MouseButtonEventArgs e)
        {
            var prev = new ImagePreview();
            NewGridFrame += prev.NewImg;
            prev.Closed += delegate { _camera.FrameReady -= prev.NewImg; };
            prev.Show();
        }

        private void ShowConfig(object sender, RoutedEventArgs e)
        {
            if(_camera != null)
                _camera.ShowConfigurationDialog();
        }

        private void MirrorCam(object sender, RoutedEventArgs e)
        {
            if (_camera != null && MirrorBtn.IsChecked.HasValue)
            {
                _camera.IsMirrored = MirrorBtn.IsChecked.Value;
                Properties.Settings.Default.Mirrored = MirrorBtn.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
        }

        private void StartCalib(object sender, RoutedEventArgs e)
        {
            _parser = new DataParser(_camera, VisualizerControl.GetVisualizer());
            //TopMost = false;
            _parser.Start();
            _parser.CalibrationComplete += CalibrationComplete;
            _parser.PenPositionChanged += PenChanged;

        }

        private void PenChanged(object sender, PenTracking.Events.PenPositionEventArgs e)
        {
            if (_emulating)
            {
                if (e == null || e.Frame == null)
                    _emulator.NoData();
                else
                {
                    _emulator.NewPoint(e.Frame.Point);
                    if (Dispatcher.CheckAccess())
                        PosTxt.Text = e.Frame.Point.ToString();
                    else
                        Dispatcher.Invoke(() => PosTxt.Text = e.Frame.Point.ToString());
                }
            }
            else if (!(e == null || e.Frame == null))
            {
                if (Dispatcher.CheckAccess())
                    PosTxt.Text = e.Frame.Point.ToString();
                else
                    Dispatcher.Invoke(() => PosTxt.Text = e.Frame.Point.ToString());
            }
        }

        private void CalibrationComplete(object sender, EventArgs e)
        {
            if (Dispatcher.CheckAccess())
                EmuBtn.IsEnabled = true;
            else
                Dispatcher.Invoke(() => EmuBtn.IsEnabled = true);
        }

        private void StartEmu(object sender, RoutedEventArgs e)
        {
            if (_emulator == null)
                _emulator = AdvancedInputEmulatorFactory.GetInputEmulator((int) SystemParameters.VirtualScreenWidth,
                                                                          (int) SystemParameters.VirtualScreenHeight);
            _emulating = EmuBtn.IsChecked.Value;
            if (EmuBtn.IsChecked.Value)
                _emulator.ShowMenu += ToggleMenu;
            else
                _emulator.ShowMenu -= ToggleMenu;
        }

        private void ToggleMenu(object sender, EventArgs e)
        {
            if (_shown)
                if (Dispatcher.CheckAccess())
                    Hide();
                else
                    Dispatcher.Invoke(() => Hide());
            else

                if (Dispatcher.CheckAccess())
                    Show();
                else
                    Dispatcher.Invoke(() => Show());
            _shown = !_shown;
        }

        public event EventHandler<FrameReadyEventArgs> NewGridFrame;

        private void ExitAll(object sender, RoutedEventArgs e)
        {
            if(_camera != null)
                _camera.Dispose();
            Application.Current.Shutdown();
        }
    }
}
