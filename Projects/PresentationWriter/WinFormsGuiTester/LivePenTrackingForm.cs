using HSR.PresentationWriter.DataSources;
using HSR.PresentationWriter.Parser;
using HSR.PresentationWriter.Parser.Strategies;
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
        private Graphics overlayGraphics;
        private StreamWriter streamWriter;

        public LivePenTrackingForm()
        {
            penDrawingBuffer = new FixedSizedQueue<PointFrame>(50);
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tracker = new AForgePenTracker(new RedLaserStrategy());

#if DEBUG
            tracker.DebugPicture += tracker_DebugPicture;
#endif

            camera = new AForgeCamera();
            camera.FrameReady += camera_FrameReady;
            camera.Start();
            camera.ShowConfigurationDialog();
        }

        //private int lastRandX = 100;
        //private int lastRandY = 100;
        //Random tempRandom = new Random();
        private async void camera_FrameReady(object sender, FrameReadyEventArgs e)
        {
            Task<PointFrame> asyncPointFrame = tracker.ProcessAsync(e.Frame);
            Bitmap redaction = (Bitmap)e.Frame.Bitmap.Clone();
            PointFrame pointFrame = await asyncPointFrame;

            //// fake:
            //lastRandX += tempRandom.Next(15);
            //lastRandY += tempRandom.Next(15);
            //penDrawingBuffer.Enqueue(new PointFrame(1, new Point(lastRandX % 640, lastRandY % 480)));

            // draw points in buffer to image
            using (Graphics g = Graphics.FromImage(redaction))
            {
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    Point previousPoint = Point.Empty;
                    foreach (PointFrame f in penDrawingBuffer)
                    {
                        g.DrawEllipse(Pens.Green, f.Point.X, f.Point.Y, 3, 3);
                        if (!previousPoint.IsEmpty)
                        {
                            g.DrawLine(Pens.Red, previousPoint, f.Point);
                        }
                        previousPoint = f.Point;
                    }
                }
                this.calibrationPictureBox.Image = redaction;
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
