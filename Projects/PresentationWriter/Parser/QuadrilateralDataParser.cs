using System;
using System.Drawing.Imaging;
using HSR.PresWriter.DataSources.Cameras;
using HSR.PresWriter.PenTracking.Events;
using HSR.PresWriter.PenTracking.Strategies;
using HSR.PresWriter.Containers;
using HSR.PresWriter.IO;
using HSR.PresWriter.IO.Events;
using System.Diagnostics;
using Visualizer;
using Point = System.Drawing.Point;

namespace HSR.PresWriter.PenTracking
{
    public class QuadrilateralDataParser
    {
        private ICalibrator _calibrator;
        private IPenTracker _penTracker;
        private IPictureProvider _provider;
        private const int TransformWidth = 640;
        private const int TransformHeight = 480;
        private QuadrilateralTransformationCamera _qtc;


        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Set up a parser gor th images
        /// Parsing all the data
        /// </summary>
        /// <param name="provider">Provider of the Camera images</param>
        /// <param name="visualizer">The Output to write on</param>
        public QuadrilateralDataParser(IPictureProvider provider, IVisualizerControl visualizer)
        {
            // Initialize Calibration Tools
            _calibrator = new SimpleAForgeCalibrator(provider, visualizer);
            _calibrator.CalibrationCompleted += StartTracking; // begin pen tracking after calibration immediately

            _provider = provider;
        }

        /// <summary>
        /// Calibration is finished, starting PenTracker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartTracking(object sender, EventArgs e)
        {
            Console.WriteLine("Calibration completed");

            _qtc = new QuadrilateralTransformationCamera(CalibratorGrid.TopLeft, CalibratorGrid.TopRight, 
                CalibratorGrid.BottomLeft, CalibratorGrid.BottomRight, 4.0/3.0);
            _qtc.Height = TransformHeight;
            _qtc.Width = TransformWidth;
            _provider.FrameReady += ForwardImages;
#if DEBUG
            _qtc.FrameReady += StoreTransformedImage;
#endif
            // Initialize Pen Tracking Tools
            _penTracker = new AForgePenTracker(new WhiteLedStrategy(), _qtc);
            _penTracker.PenFound += PenFound;
            _penTracker.Start();
            //_calibrator.Grid.PredictFromCorners();
        }

        private void StoreTransformedImage(object sender, FrameReadyEventArgs e)
        {
            //e.Frame.Bitmap.Save(@"C:\temp\daforge\transformation\img" + e.Frame.Number + ".jpg", ImageFormat.Jpeg);
        }

        private void ForwardImages(object sender, FrameReadyEventArgs e)
        {
            _qtc.TransformImage(e.Frame);
        }

        /// <summary>
        /// Starting calibration and start tracking the pen afterwards
        /// </summary>
        public void Start()
        {
            IsRunning = true;
            _calibrator.CalibrationCompleted += StartTracking;
            _calibrator.Calibrate();
        }

        /// <summary>
        /// Stopping the PenTracker
        /// <remarks>Cannot be used to stop the calibration, this have to be completed first</remarks>
        /// </summary>
        public void Stop()
        {
            _penTracker.Stop();
            IsRunning = false;
        }

        /// <summary>
        /// Mapping the point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PenFound(object sender, PenFoundEventArgs e)
        {
            Debug.WriteLine("Pen Nr\t{0} at {1},{2}", e.Frame.Number, e.Frame.Point.X, e.Frame.Point.Y);
            Point point = e.Frame.Point;
            double distortionX = (double)CalibratorGrid.ScreenSize.Width / (double)TransformWidth;
            double distortionY = (double)CalibratorGrid.ScreenSize.Height / (double)TransformHeight;
            //point.X *= (int)Math.Round((double)CalibratorGrid.ScreenSize.Width / TransformWidth);
            //point.Y *= (int)Math.Round((double)CalibratorGrid.ScreenSize.Height / TransformHeight);

            point.X = (int)Math.Round(distortionX * (double)e.Frame.Point.X);
            point.Y = (int)Math.Round(distortionY * (double)e.Frame.Point.Y);

            PointFrame frame = e.Frame.ApplyRebase(point);
            if (PenPositionChanged != null)
                PenPositionChanged(this, new VirtualPenPositionEventArgs(frame, true));
        }

        public Grid CalibratorGrid { get { return _calibrator.Grid; } }

        /// <summary>
        /// The pen changed the position
        /// </summary>
        public event EventHandler<PenPositionEventArgs> PenPositionChanged;
    }
}
