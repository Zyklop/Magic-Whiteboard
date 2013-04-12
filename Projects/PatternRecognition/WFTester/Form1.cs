using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFTester
{
    public partial class Form1 : Form
    {
        private List<Bitmap> pictureList = new List<Bitmap>();
        private int pictureIndex = 0;

        public Form1()
        {
            //pictureList = BaseRecognition.Recognize();
            pictureList = BaseRecognition.RecognizeLines();

            InitializeComponent();

            this.pictureBox1.Image = pictureList[pictureIndex++];
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (pictureList.Count != 0)
            {
                if (pictureIndex >= pictureList.Count)
                {
                    pictureIndex = 0;
                }
                this.pictureBox1.Image = pictureList[pictureIndex];
                pictureIndex++;
            }
        }
    }
}
