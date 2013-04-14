using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using HSR.PresWriter.Containers;
using HSR.PresWriter.IO;
using HSR.PresWriter.IO.Events;
using Visualizer;

namespace HSR.PresWriter.DataSources.Cameras
{
    public class VirtualCamera : ICamera, IVisualizerControl
    {
        private Bitmap _bm;
        private Graphics _g;
        private Timer _tim = new Timer();
        private static int counter = 10;
        private bool queued;

        public VirtualCamera()
        {
            _tim = new Timer();
            _tim.Interval = 2000;
            _tim.AutoReset = false;
            _tim.Elapsed += NoData;
            _bm = new Bitmap(Width, Height);
            _g = Graphics.FromImage(_bm);
        }

        private void NoData(object sender, ElapsedEventArgs e)
        {
            if (!queued)
            {
                var bm = new Bitmap(640, 480);
                var g = Graphics.FromImage(bm);
                g.Clear(Color.DarkGray);
                if (FrameReady != null) FrameReady(this, new FrameReadyEventArgs(new VideoFrame(counter++, bm)));
                g.Dispose();
            }
        }

        public event EventHandler<FrameReadyEventArgs> FrameReady;

        public void Dispose()
        {
            // nothing
        }

        public void Start()
        {
            _tim.Start();
            IsRunning = true;
        }

        public void Stop()
        {
            _tim.Stop();
            IsRunning = false;
        }

        public bool IsRunning { get; private set; }

        public VideoFrame GetLastFrame()
        {
            throw new NotImplementedException();
        }

        public bool Transparent { get; set; }

        public int Width
        {
            get { return 1280; }
        }

        public int Height
        {
            get { return 1024; }
        }


        public void AddRect(System.Drawing.Point topLeft, System.Drawing.Point bottomRight, Color fromRgb)
        {
            lock (_g)
            {
                _g.FillRectangle(new SolidBrush(fromRgb), topLeft.X, topLeft.Y, bottomRight.X - topLeft.X,
                                 bottomRight.Y - topLeft.Y);
                _g.Flush();
            }
        }

        public void Clear()
        {
            _g.Clear(Color.Black);
        }

        public void Close()
        {
            // impossible
        }

        public void Show()
        {
            // impossible
        }

        public void AddLine(System.Drawing.Point topLeft, System.Drawing.Point bottomRight, Color fromRgb)
        {
            lock (_g)
            {
                _g.DrawLine(new Pen(fromRgb), topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
                _g.Flush();
            }
        }

        public async void Draw()
        {
            var bm = new Bitmap(640, 480);
            var g = Graphics.FromImage(bm);
            queued = true;
            //_tim.Stop();
            lock (_g)
            {
                g.Clear(Color.DarkGray);
                g.DrawImage(_bm, 40, 60, 400, 300);
            }
            await Task.Delay(1000);
            if (FrameReady != null) FrameReady(this, new FrameReadyEventArgs(new VideoFrame(counter++, bm)));
            g.Dispose();
            queued = false;
            //_tim.Start();
        }

        public void ClearRects()
        {
            lock (_g)
            {
                _g.Clear(Color.Black);
            }
        }

        public void AddRect(int topLeft, int bottomRight, int width, int height, Color color)
        {
            //var g = CreateGraphics();
            //var r = new Rectangle(topLeft, bottomRight, width, height);
            //g.FillRectangles(new SolidBrush(color), new Rectangle[1] { r });
            //OnPaint(new PaintEventArgs(g,r));
            lock (_g)
            {
                _g.FillRectangle(new SolidBrush(color), topLeft, bottomRight, width, height);
                _g.Flush();
            }
        }

        //internal void AddCirclesAround(int x, int y)
        //{
        //    lock (_g)
        //    {
        //        _g.DrawEllipse(new Pen(Color.Red), x - 200, y - 200, 400, 400);
        //        _g.DrawEllipse(new Pen(Color.Red), x - 100, y - 100, 200, 200);
        //        _g.Flush();
        //    }
        //    Invalidate(new Rectangle(x - 200, y - 200, 400, 400));
        //}
    }
}
