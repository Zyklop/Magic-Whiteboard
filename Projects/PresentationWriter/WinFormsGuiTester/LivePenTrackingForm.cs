using HSR.PresWriter.Containers;
using HSR.PresWriter.IO;
using HSR.PresWriter.IO.Cameras;
using HSR.PresWriter.IO.Events;
using HSR.PresWriter.PenTracking;
using HSR.PresWriter.PenTracking.Events;
using HSR.PresWriter.PenTracking.Strategies;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsGuiTester
{
    public partial class LivePenTrackingForm : Form
    {
        private FixedSizedQueue<PointFrame> penDrawingBuffer;
        private AForgeCamera camera;
        private AForgePenTracker tracker;
        private Bitmap _bitmap;

        public LivePenTrackingForm()
        {
            penDrawingBuffer = new FixedSizedQueue<PointFrame>(100);
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            camera = new AForgeCamera();
            camera.FrameReady += camera_FrameReady;
            camera.Start();
            camera.ShowConfigurationDialog();

            tracker = new AForgePenTracker(new WhiteLedStrategy(), camera);
            tracker.PenFound += Found;
            tracker.Start();

#if DEBUG
            tracker.DebugPicture += tracker_DebugPicture;
#endif
        }

        private void camera_FrameReady(object sender, FrameReadyEventArgs e)
        {
            _bitmap = (Bitmap)e.Frame.Bitmap.Clone();
        }

        //private int lastRandX = 100;
        //private int lastRandY = 100;
        //Random tempRandom = new Random();
        private async void Found(object sender, PenPositionEventArgs penPositionEventArgs)
        {
            //Bitmap redaction = (Bitmap)e.Frame.Bitmap.Clone();
            PointFrame pointFrame = penPositionEventArgs.Frame;

            if (pointFrame != null)
            {
                penDrawingBuffer.Enqueue(pointFrame);
            }

            // draw points in buffer to image
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    Point previousPoint = Point.Empty;
                    foreach (PointFrame f in penDrawingBuffer)
                    {
                        g.DrawEllipse(Pens.Green, f.Point.X -3, f.Point.Y -3, 3, 3);
                        if (!previousPoint.IsEmpty && PointTools.CalculateDistance(previousPoint, f.Point) < 50)
                        {
                            g.DrawLine(Pens.Red, previousPoint, f.Point);
                        }
                        previousPoint = f.Point;
                    }
                }
                g.Save();
                this.calibrationPictureBox.Image = _bitmap;
            }
        }

#if DEBUG
        private void tracker_DebugPicture(object sender, DebugPictureEventArgs e)
        {
            this.diffDebugPicture.Image = (Image)e.Pictures[0].Clone();
            this.blobDebugPicture.Image = (Image)e.Pictures[2].Clone();
        }
#endif


        private void LivePenTrackingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if DEBUG
            tracker.DebugPicture -= tracker_DebugPicture;
#endif
            camera.FrameReady -= camera_FrameReady;
            camera.Stop();
        }
    }
}
