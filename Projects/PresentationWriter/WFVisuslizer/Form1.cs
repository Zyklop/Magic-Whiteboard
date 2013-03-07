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
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.Black;
        }

        public bool Transparent
        {
            get { return BackColor == Color.Transparent; }
            set
        {
            if (value)
            {
                BackColor = Color.Transparent;
            }
            else
            {
                BackColor = Color.Black;
            }
        } }


        public void AddRect(Point topLeft, Point bottomRight, Color fromRgb)
        {
            var g = this.CreateGraphics();
            g.FillRectangle(new SolidBrush(fromRgb), topLeft.X, topLeft.Y, bottomRight.X-topLeft.X, bottomRight.Y-topLeft.Y);
            Invalidate();
            g.Dispose();
        }

        public void ClearRects()
        {
            var g = this.CreateGraphics();
            g.Clear(BackColor);
            Invalidate();
            g.Dispose();
        }

        public void AddRect(int topLeft, int bottomRight, int width, int height, Color color)
        {
            var g = this.CreateGraphics();
            g.FillRectangle(new SolidBrush(color), topLeft, bottomRight, width, height);
            Invalidate();
            g.Dispose();
        }

        public void AddRect(System.Windows.Point topLeft, System.Windows.Point bottomRight, Color fromRgb)
        {
            var g = this.CreateGraphics();
            g.FillRectangle(new SolidBrush(fromRgb), (int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y);
            Invalidate();
            g.Dispose();
        }
    }
}
