using HSR.PresentationWriter.DataSources;
using HSR.PresentationWriter.Parser;
using HSR.PresentationWriter.Parser.Strategies;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WinFormsGuiTester
{
    public partial class LivePenTrackingForm : Form
    {
        private AForgeCamera camera;
        private AForgePenTracker tracker;
        private Graphics overlayGraphics;
        private StreamWriter streamWriter;

        public LivePenTrackingForm()
        {
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

        private async void camera_FrameReady(object sender, FrameReadyEventArgs e)
        {
            long time1 = CurrentMillis.Millis;
            PointFrame p = await tracker.ProcessAsync(e.Frame);
            long time2 = CurrentMillis.Millis;
            Debug.WriteLine("Time: {0}", time2 - time1);
            //PointFrame p = tracker.GetLastFrame();
            this.calibrationPictureBox.Image = (Image)e.Frame.Bitmap.Clone();
        }

#if DEBUG
        private void tracker_DebugPicture(object sender, DebugPictureEventArgs e)
        {
            this.diffDebugPicture.Image = (Image)e.Pictures[0].Clone();
            this.blobDebugPicture.Image = (Image)e.Pictures[1].Clone();
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
