using System;
using HSR.PresWriter.Common.IO;
using HSR.PresWriter.PenTracking.Calibrators;
using HSR.PresWriter.PenTracking.Events;
using HSR.PresWriter.PenTracking.Mappers;
using HSR.PresWriter.PenTracking.Strategies;
using HSR.PresWriter.Visualizer;

namespace HSR.PresWriter.PenTracking
{
    public class DataParser
    {
        private readonly ICalibrator _calibrator; // TODO Interface anpassen an StartCalibration etc
        private readonly IPenTracker _penTracker;
        private readonly Type _mapperType;
        private AbstractPointMapper _mapper;

        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Set up a parser gor th images
        /// Parsing all the data
        /// </summary>
        /// <param name="provider">Provider of the Camera images</param>
        /// <param name="visualizer">The Output to write on</param>
        public DataParser(IPictureProvider provider, IVisualizerControl visualizer)
        {
            // Initialize Calibration Tools
            _calibrator = new SimpleAForgeCalibrator(provider, visualizer);
            _calibrator.CalibrationCompleted += StartTracking; // begin pen tracking after calibration immediately

            // Initialize Pen Tracking Tools
            _penTracker = new AForgePenTracker(new WhiteLedStrategy(), provider);
            _penTracker.PenFound += PenFound;
            _penTracker.NoPenFound += NoPenFound;

            _mapperType = typeof(BarycentricIntegralPointMapper);
        }

        private void NoPenFound(object sender, EventArgs e)
        {
            if (PenPositionChanged != null)
            {
                PenPositionChanged(this, new VirtualPenPositionEventArgs(null, false));
            }
        }

        /// <summary>
        /// Calibration is finished, starting PenTracker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartTracking(object sender, EventArgs e)
        {
            var mCtor = _mapperType.GetConstructor(new Type[] { typeof(Grid) });
            _mapper = (AbstractPointMapper)mCtor.Invoke(new Grid[] { CalibratorGrid });
            Console.WriteLine("Calbration completed");
            _penTracker.Start();
            if(CalibrationComplete != null)
                CalibrationComplete(this, null);
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
        /// Restart the Parser again, without recalibrating
        /// </summary>
        public void Restart()
        {
            if(CalibratorGrid.TopLeft.IsEmpty)
                throw new InvalidOperationException("Needs to be Calibrated first. Use Start()");
            _penTracker.Start();
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
        /// Start the Calibration again
        /// </summary>
        public void ReCalibrate()
        {
            // TODO not working, why?
            _penTracker.Stop();
            _calibrator.Calibrate();
            Console.WriteLine("Recalibrating");
        }

        /// <summary>
        /// Mapping the point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PenFound(object sender, PenFoundEventArgs e)
        {
            var point = _mapper.FromPresentation(e.Frame.Point.X, e.Frame.Point.Y);
            var frame = e.Frame.ApplyRebase(point);
            if (PenPositionChanged != null)
            {
                PenPositionChanged(this, new VirtualPenPositionEventArgs(frame, CalibratorGrid.Contains(point)));
            }
        }

        /// <summary>
        /// Grid of the Calibrator
        /// </summary>
        public Grid CalibratorGrid { get { return _calibrator.Grid; } }

        /// <summary>
        /// The pen changed the position
        /// </summary>
        public event EventHandler<PenPositionEventArgs> PenPositionChanged;

        public event EventHandler CalibrationComplete;
    }
}
