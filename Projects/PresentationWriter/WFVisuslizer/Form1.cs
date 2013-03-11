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

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.Black;
            _bm = new Bitmap(Width,Height);
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
            _g.FillRectangle(new SolidBrush(fromRgb), topLeft.X, topLeft.Y, bottomRight.X-topLeft.X, bottomRight.Y-topLeft.Y);
            _g.Flush();
            Invalidate();
            Application.DoEvents();
        }

        public void ClearRects()
        {
            _g.Clear(BackColor);
            Invalidate();
            Application.DoEvents();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawImage(_bm, 0, 0);
        }

        public void AddRect(int topLeft, int bottomRight, int width, int height, Color color)
        {
            //var g = CreateGraphics();
            //var r = new Rectangle(topLeft, bottomRight, width, height);
            //g.FillRectangles(new SolidBrush(color), new Rectangle[1] { r });
            //OnPaint(new PaintEventArgs(g,r));
            _g.FillRectangle(new SolidBrush(color), topLeft, bottomRight, width, height);
            _g.Flush();
            Invalidate();
            Application.DoEvents();
            //g.Dispose();
        }

        public void AddRect(System.Windows.Point topLeft, System.Windows.Point bottomRight, Color fromRgb)
        {
            //var g = this.CreateGraphics();
            _g.FillRectangle(new SolidBrush(fromRgb), (int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y);
            _g.Flush();
            Invalidate();
            Application.DoEvents();
            //g.Dispose();
        }
    }
}
