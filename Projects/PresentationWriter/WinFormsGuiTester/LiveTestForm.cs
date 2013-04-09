using HSR.PresWriter.Containers;
using HSR.PresWriter.IO;
using HSR.PresWriter.IO.Cameras;
using HSR.PresWriter.IO.Events;
using HSR.PresWriter.PenTracking;
using HSR.PresWriter.PenTracking.Events;
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
    public partial class LiveTestForm : Form
    {
        private AForgeCamera _camera;
        private DataParser _parser;
        private FixedSizedQueue<PointFrame> penDrawingBuffer;

        public LiveTestForm()
        {
            this.penDrawingBuffer = new FixedSizedQueue<PointFrame>(100);
            InitializeComponent();

            // Initialize Camera
            _camera = new AForgeCamera();
            //_camera = new FilesystemCamera(new DirectoryInfo(@"C:\temp\aforge\inph"));
            _camera.FrameReady += _camera_FrameReady;

            // Initialize Calibration and Pen Parsing Mechanism
            _parser = new DataParser(_camera);
            _parser.PenPositionChanged += parser_PenPositionChanged;
        }

        private void _camera_FrameReady(object sender, FrameReadyEventArgs e)
        {
            Bitmap redaction = (Bitmap)e.Frame.Bitmap.Clone();

            // draw points in buffer to image
            using (Graphics g = Graphics.FromImage(redaction))
            {
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    Point bottomLeft = _parser.CalibratorGrid.BottomLeft;
                    Point topLeft = _parser.CalibratorGrid.TopLeft;
                    Point bottomRight = _parser.CalibratorGrid.BottomRight;
                    Point topRight = _parser.CalibratorGrid.TopRight;
                    g.DrawLine(Pens.Red, bottomLeft, bottomRight);
                    g.DrawLine(Pens.Red, bottomLeft, topLeft);
                    g.DrawLine(Pens.Red, topRight, bottomRight);
                    g.DrawLine(Pens.Red, topRight, topLeft);
                }
                this.cameraPictureBox.Image = redaction;
            }

        }

        private void parser_PenPositionChanged(object sender, PenPositionEventArgs e)
        {
            Bitmap redaction = (Bitmap)this.cameraPictureBox.Image.Clone();

            if (e.Frame != null)
            {
                penDrawingBuffer.Enqueue(e.Frame);
            }
            // draw points in buffer to image
            using (Graphics g = Graphics.FromImage(redaction))
            {
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    Point previousPoint = Point.Empty;
                    foreach (PointFrame f in penDrawingBuffer)
                    {
                        g.DrawEllipse(Pens.Green, f.Point.X - 3, f.Point.Y - 3, 3, 3);
                        if (!previousPoint.IsEmpty && PointTools.CalculateDistance(previousPoint, f.Point) < 50)
                        {
                            g.DrawLine(Pens.Red, previousPoint, f.Point);
                        }
                        previousPoint = f.Point;
                    }
                }
                this.cameraPictureBox.Image = redaction;
            }

            this.foundPointLabel.Text = "Found Point: " + e.Frame.Point.X + ", " + e.Frame.Point.Y;
        }

        private void toggleCameraButton_Click(object sender, EventArgs e)
        {
            if (!_camera.IsRunning)
            {
                _camera.Start();
                this.toggleCameraButton.Text = "Stop Camera";
            }
            else
            {
                _camera.Stop();
                this.toggleCameraButton.Text = "Start Camera";
            }
        }

        private void toggleParserButton_Click(object sender, EventArgs e)
        {
            if (!_parser.IsRunning)
            {
                _parser.Start();
                this.toggleParserButton.Text = "Stop Parser";
            }
            else
            {
                _parser.Stop();
                this.toggleParserButton.Text = "Start Parser";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _camera.ShowConfigurationDialog();
        }
    }
}
