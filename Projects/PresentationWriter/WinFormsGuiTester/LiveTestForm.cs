using HSR.PresWriter.IO.Cameras;
using HSR.PresWriter.PenTracking;
using HSR.PresWriter.PenTracking.Events;
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
    public partial class LiveTestForm : Form
    {
        private AForgeCamera _camera;
        private DataParser _parser;

        public LiveTestForm()
        {
            InitializeComponent();

            // Initialize Camera
            _camera = new AForgeCamera();
            _camera.FrameReady += _camera_FrameReady;

            // Initialize Calibration and Pen Parsing Mechanism
            _parser = new DataParser(_camera);
            _parser.PenPositionChanged += parser_PenPositionChanged;
        }

        private void _camera_FrameReady(object sender, HSR.PresWriter.IO.Events.FrameReadyEventArgs e)
        {
            this.cameraPictureBox.Image = (Bitmap)e.Frame.Bitmap.Clone();
        }

        private void parser_PenPositionChanged(object sender, PenPositionEventArgs e)
        {
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
    }
}
