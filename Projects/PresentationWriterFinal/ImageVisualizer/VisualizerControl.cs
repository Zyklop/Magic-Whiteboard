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
        private VisualizerApp _app;

        public static VisualizerControl GetVisualizer()
        {
            return _singleton ?? (_singleton = new VisualizerControl());
        }

        protected VisualizerControl()
        {
            _t = new Thread(() =>
            {
                _app = new VisualizerApp();
                _cw = _app.CalibratorWindow;
                _app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                _app.Run();
            });
            _t.SetApartmentState(ApartmentState.STA);
            _t.Start();
        }

        public bool Transparent { get { return _cw.Transparent; } set { _cw.Transparent = value; } }

        public void AddRect(Point topLeft, Point bottomRight, System.Drawing.Color fromRgb)
        {
            _cw.AddRect(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y, Color.FromRgb(fromRgb.R,fromRgb.G,fromRgb.B));
        }

        public void Clear()
        {
            _cw.Clear();
        }

        public void Draw()
        {
            //not needed
        }

        public void AddRect(int topLeft, int bottomRight, int width, int height, System.Drawing.Color color)
        {
            _cw.AddRect(topLeft, bottomRight, width, height, Color.FromRgb(color.R, color.G, color.B));
        }

        public void Close()
        {
            if (_cw.Dispatcher.CheckAccess())
                _cw.Hide();
            else
                _cw.Dispatcher.BeginInvoke(new Action(() => _cw.Hide()));
        }

        public void Show()
        {
            if(_cw.Dispatcher.CheckAccess())
                _cw.Show();
            else
                _cw.Dispatcher.BeginInvoke(new Action(()=> _cw.Show()));
        }

        public int Width { get { return (int) _cw.ActualWidth; } }

        public int Height { get { return (int) _cw.ActualHeight; } }
    }
}
