using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFVisuslizer
{
    public partial class Form1 : Form
    {
        private Bitmap _bm;
        private Graphics _g;
        private static int _counter = 10;
        //[BrowsableAttribute(false)]
        //public static bool CheckForIllegalCrossThreadCalls { get; set; }


        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            CheckForIllegalCrossThreadCalls = false;
            BackColor = Color.Black;
            _bm = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            _g = Graphics.FromImage(_bm);
        }

        public bool Transparent
        {
            get { return BackColor == Color.Transparent; }
            set
        {
            if (value)
            {
                TransparencyKey = Color.Black;
            }
            else
            {
                TransparencyKey = Color.Transparent;
            }
                Application.DoEvents();
        } }


        public void AddRect(Point topLeft, Point bottomRight, Color fromRgb)
        {
            lock (_g)
            {
                _g.FillRectangle(new SolidBrush(fromRgb), topLeft.X, topLeft.Y, bottomRight.X-topLeft.X, bottomRight.Y-topLeft.Y);
                _g.Flush();
            }
            //Invalidate(new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y));
            Application.DoEvents();
        }

        public void AddLine(Point topLeft, Point bottomRight, Color fromRgb)
        {
            lock (_g)
            {
                _g.DrawLine(new Pen(fromRgb), topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
                _g.Flush();
            }
            //Invalidate(new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y));
            Application.DoEvents();
        }

        public void Draw()
        {
            Invalidate();
#if DEBUG
            lock (_g)
            {

                var bm = new Bitmap(Screen.PrimaryScreen.Bounds.Width + 100, Screen.PrimaryScreen.Bounds.Height + 100);
                var g = Graphics.FromImage(bm);
                g.Clear(Color.DarkGray);
                g.DrawImageUnscaled(_bm, 40, 60);
                bm.Save(@"C:\temp\daforge\outp\img" + _counter++ + ".jpg");
            }
#endif
        }

        public void ClearRects()
        {
            lock (_g)
            {
                _g.Clear(BackColor);
            }
            Invalidate();
            Application.DoEvents();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            lock (_g)
            {
                base.OnPaint(e);
                e.Graphics.DrawImageUnscaled(_bm, 0, 0);
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
            //Invalidate(new Rectangle(topLeft, bottomRight, width, height));
            Application.DoEvents();
            //g.Dispose();
        }

        public void AddRect(System.Windows.Point topLeft, System.Windows.Point bottomRight, Color fromRgb)
        {
            //var g = this.CreateGraphics();
            lock (_g)
            {
                _g.FillRectangle(new SolidBrush(fromRgb), (int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y);
                _g.Flush();
            }
            //Invalidate(new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y));
            Application.DoEvents();
            //g.Dispose();
        }

        internal void AddCirclesAround(int x, int y)
        {
            lock (_g)
            {
                _g.DrawEllipse(new Pen(Color.Red), x - 200, y - 200, 400, 400);
                _g.DrawEllipse(new Pen(Color.Red), x - 100, y - 100, 200, 200);
                _g.Flush();
            }
            Invalidate(new Rectangle(x - 200, y - 200, 400, 400));
            Application.DoEvents();
        }
    }
}
