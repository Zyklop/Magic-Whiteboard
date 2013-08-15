using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using HSR.PresWriter.Visualizer;
using Point = System.Drawing.Point;

namespace HSR.PresWriter.ImageVisualizer
{
    public class VisualizerControl:IVisualizerControl
    {
        private CalibratorWindow _cw;
        private static VisualizerControl _singleton;
        private readonly Thread _t;
        private Application _app;

        /// <summary>
        /// Return the actual visualizer
        /// </summary>
        /// <returns></returns>
        public static VisualizerControl GetVisualizer()
        {
            return _singleton ?? (_singleton = new VisualizerControl());
        }

        /// <summary>
        /// Create a WPF Application if needed
        /// </summary>
        protected VisualizerControl()
        {
            if (Application.Current == null)
            {
                //started form a non wpf Context
                _t = new Thread(() =>
                    {
                        _app = new VisualizerApp();
                        _cw = ((VisualizerApp)_app).CalibratorWindow;
                        _app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                        _app.Run();
                    });
                _t.SetApartmentState(ApartmentState.STA);
                _t.Start();
            }
            else
            {
                //started form a WPF application
                _app = Application.Current;
                _cw = new CalibratorWindow();
                _cw.Show();
                _cw.Hide();
            }
        }

        /// <summary>
        /// Set the transparency
        /// </summary>
        public bool Transparent { get { return _cw.Transparent; } set { _cw.Transparent = value; } }

        public void AddRect(Point topLeft, Point bottomRight, System.Drawing.Color fromRgb)
        {
            _cw.AddRect(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y, Color.FromRgb(fromRgb.R,fromRgb.G,fromRgb.B));
        }

        /// <summary>
        /// remove all content
        /// </summary>
        public void Clear()
        {
            _cw.Clear();
        }

        /// <summary>
        /// Reender new Content under WinGorms
        /// </summary>
        public void Draw()
        {
            //not needed
        }

        /// <summary>
        /// Draw a filled rectangle
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public void AddRect(int topLeft, int bottomRight, int width, int height, System.Drawing.Color color)
        {
            _cw.AddRect(topLeft, bottomRight, width, height, Color.FromRgb(color.R, color.G, color.B));
        }

        /// <summary>
        /// Hide the Window
        /// </summary>
        public void Close()
        {
            if (_cw.Dispatcher.CheckAccess())
                _cw.Hide();
            else
                _cw.Dispatcher.BeginInvoke(new Action(() => _cw.Hide()));
        }

        /// <summary>
        /// Show the window
        /// </summary>
        public void Show()
        {
            if(_cw.Dispatcher.CheckAccess())
                _cw.Show();
            else
                _cw.Dispatcher.BeginInvoke(new Action(()=> _cw.Show()));
        }

        /// <summary>
        /// Screen width
        /// </summary>
        //public int Width { get { return (int) _cw.ActualWidth; } }
        public int Width { get { return (int)SystemParameters.VirtualScreenWidth; } }

        /// <summary>
        /// Screen Height
        /// </summary>
        //public int Height { get { return (int) _cw.ActualHeight; } }
        public int Height { get { return (int)SystemParameters.VirtualScreenHeight; } }
    }
}
