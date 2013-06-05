using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsGuiTester
{
    public partial class MeasureGrid : Form
    {
        public MeasureGrid()
        {
            InitializeComponent();

            Rectangle resolution = Screen.PrimaryScreen.Bounds;
            double res = (double)resolution.Width / (double)resolution.Height;
            resolutionLabel.Text = resolution.Width + "x" + resolution.Height + " (" + res + ":1)";
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
