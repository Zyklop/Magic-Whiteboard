using HSR.PresWriter.Containers;
using HSR.PresWriter.PenTracking;
using HSR.PresWriter.PenTracking.Strategies;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsGuiTester
{
    public partial class LibraryTrackingForm : Form
    {
        private List<PointFrame> penDrawingBuffer;

        public LibraryTrackingForm()
        {
            penDrawingBuffer = new List<PointFrame>();
            InitializeComponent();
            updateListBox(@"c:\temp\filtercalib");
        }

        private void updateListBox(string path)
        {
            this.inputListBox.Items.Clear();
            DirectoryInfo d = new DirectoryInfo(path);
            foreach (FileInfo f in d.GetFiles("*.bmp"))
            {
                this.inputListBox.Items.Add(f.FullName);
            }
            foreach (FileInfo f in d.GetFiles("*.jpg"))
            {
                this.inputListBox.Items.Add(f.FullName);
            }
            foreach (FileInfo f in d.GetFiles("*.png"))
            {
                this.inputListBox.Items.Add(f.FullName);
            }
        }


        private async void processAll()
        {
            penDrawingBuffer.Clear();
            //AForgePenTracker tracker = new AForgePenTracker(new LightBulbStrategy(), ); //TODO dunno how to change
            int framenumber = 0;
            foreach (String s in this.inputListBox.Items)
            {
                VideoFrame videoFrame = new VideoFrame(++framenumber, new Bitmap(s));
                //PointFrame pointFrame = await tracker.ProcessAsync(videoFrame); //Todo
                //penDrawingBuffer.Add(pointFrame);
            }
        }

        private void folderButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog d = new FolderBrowserDialog();
            if (d.ShowDialog() == DialogResult.OK)
            {
                this.pathLabel.Text = d.SelectedPath;
                updateListBox(d.SelectedPath);
            }
        }

        private void inputListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string path = (string)((ListBox)sender).SelectedItem;
            int framenumber = ((ListBox)sender).SelectedIndex;
            this.framePictureBox.Image = new Bitmap(path);

            using (Graphics g = Graphics.FromImage(this.framePictureBox.Image))
            {
                Point previousPoint = Point.Empty;
                int i = 0;
                foreach (PointFrame f in penDrawingBuffer)
                {
                    if (f != null)
                    {
                        if (!previousPoint.IsEmpty)
                        {
                            g.DrawLine(Pens.Red, previousPoint, f.Point);
                        }
                        g.DrawEllipse(Pens.Green, f.Point.X-2, f.Point.Y-2, 3, 3);
                        previousPoint = f.Point;
                    }
                    i++;
                    if (i > framenumber)
                    {
                        break;
                    }
                }
            }
        }

        private void processButton_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            processAll();
            this.Cursor = Cursors.Default;
        }
    }
}
