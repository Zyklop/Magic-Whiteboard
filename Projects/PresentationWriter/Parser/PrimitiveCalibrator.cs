using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Threading.Tasks;
using HSR.PresWriter.PenTracking.Events;
using HSR.PresWriter.PenTracking.Images;
using DColor = System.Drawing.Color;
using VPoint = Visualizer.Point;
using WFVisuslizer;

namespace HSR.PresWriter.PenTracking
{
    internal interface ICalibrator
    {
        /// <summary>
        /// Calibrating colors
        /// </summary>
        //TODO implement or remove
        void CalibrateColors();

        /// <summary>
        /// Calibrate the grid
        /// </summary>
        void Calibrate();

        /// <summary>
        /// Check if the grid needs to be recalibrated
        /// Check if something moved
        /// </summary>
        /// <returns>
        /// 0: ok
        /// 1: grid calibration needed
        /// 2: color calibration needed
        /// 3: both needed
        /// </returns>
        int CheckCalibration();

        /// <summary>
        /// Data for the matching
        /// </summary>
        Grid Grid { get; }

        /// <summary>
        /// Data from color calibration
        /// </summary>
        Colorfilter ColorFilter { get; }
    }

    internal class PrimitiveCalibrator : ICalibrator
    {
        private const byte GreyDiff = 40;
        private const byte GreenDiff = 80;
        private const byte BlueDiff = 80;
        private const byte RedDiff = 90;
        private const int Blocksize = 10;
        private const int Blockfill = 80; // Number of pixels needed for a Block to be valid. Depends on Blocksize.
        private const int CalibrationFrames = 300; // Number of Frames used for calibration. Divide by 10 to get Time for calibration.
        private ThreeChannelBitmap _blackImage;
        private readonly VisualizerControl _vs = VisualizerControl.GetVisualizer();
        private int _calibrationStep;
        private int _errors;
        private readonly Rect[] _rects = new Rect[3];
        private readonly object _lockObj = new object();
        private SemaphoreSlim _sem;
        private Task<Task> _t = new Task<Task>(()=>Task.Delay(10));
        //private Task calibTask;

        /// <summary>
        /// A calibrator is needed to get the calibration grid
        /// </summary>
        /// <param name="cc"></param>
        public PrimitiveCalibrator()
        {
            Grid = new Grid(0,0);
            //var thread = new Thread(() => _vs = new CalibratorWindow());
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
            //thread.Join();
            Calibrate();
            CalibrateColors();
        }

        /// <summary>
        /// Calibrating colors
        /// </summary>
        //TODO implement or remove
        public void CalibrateColors()
        {
        }

        /// <summary>
        /// Calibrate the grid
        /// </summary>
        public void Calibrate()
        {
            _calibrationStep = 0;
            _errors = 0;
            _sem = new SemaphoreSlim(1, 1);
            // _cc.NewImage += BaseCalibration; // TODO
        }

        private void BaseCalibration(object sender, NewImageEventArgs e)
        {
            //var thread = new Thread(() => CalibThread(e));
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
            //thread.Join();
            //_vs.Dispatcher.Invoke(() => CalibThread(e));
            //_vs.Dispatcher.InvokeAsync(() => 
            //Debug.WriteLine("Calibrating"),DispatcherPriority.Send);
            //Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
            //           new Action(() => { }));
            //_vs.Dispatcher.InvokeAsync(() => CalibThread(e));
            //if (calibTask == null || calibTask.Status != TaskStatus.Running)
            //{
            //    Task.Factory.StartNew(() => { calibTask = CalibThread(e); });
            //    Debug.WriteLine("thread started");
            //    //calibTask = CalibThread(e);
            //}
            //Debug.WriteLine(calibTask.Status);
            //Task.Factory.StartNew(() => CalibThread(e));
                Debug.WriteLine("new image, sem = "+_sem.CurrentCount+" "+_t.Status);
            if (_sem.CurrentCount >= 1)//_t.Status != TaskStatus.Running)//
            {
                _sem.Wait();
                Task.Factory.StartNew(() => CalibThread(e));
            }
        }


        private async Task CalibThread(NewImageEventArgs e)
        {
#if DEBUG
            e.NewImage.Save(@"C:\temp\srcimg\img" + _calibrationStep + ".jpg");
#endif
            //lock (_lockObj)
            {
                Debug.WriteLine("Calibrating");
                if (_errors > 100)
                {
                    //calibration not possible
                    return;
                }
                if (_calibrationStep == CalibrationFrames || !Grid.DataNeeded)
                {
                    //_cc.NewImage -= BaseCalibration; // TODO
                    Grid.Calculate();
                    _vs.Close();
                    Debug.WriteLine("errors");
                }
                    //else if (_calibrationStep > CalibrationFrames/2)
                    //{
                
                    //}
                else if (_calibrationStep > 2)
                {
                    var tcb = ThreeChannelBitmap.FromBitmap(e.NewImage);
                    _vs.ClearRects();
                    FillRandomRects();
                    for (int j = 0; j < 3; j++)
                    {
                        var cChan = new OneChannelBitmap();
                        var diff = new BinaryBitmap();
                        Debug.WriteLine(j);
                        switch (j)
                        {
                            case 0:
                                cChan = _blackImage.RChannelBitmap - tcb.RChannelBitmap;
                                diff = cChan.GetBinary(RedDiff);
#if DEBUG
                                var s = new ThreeChannelBitmap(cChan, new OneChannelBitmap(diff.Width, diff.Height),
                                                               new OneChannelBitmap(diff.Width, diff.Height));
                                s.GetVisual().Save(@"C:\temp\rimg\img" + _calibrationStep + ".jpg");
#endif
                                break;
                            case 1:
                                cChan = _blackImage.GChannelBitmap - tcb.GChannelBitmap;
                                diff = cChan.GetBinary(GreenDiff);
#if DEBUG
                                s = new ThreeChannelBitmap(new OneChannelBitmap(diff.Width, diff.Height),cChan, 
                                                               new OneChannelBitmap(diff.Width, diff.Height));
                                s.GetVisual().Save(@"C:\temp\gimg\img" + _calibrationStep + ".jpg");
#endif
                                break;
                            case 2:
                                cChan = _blackImage.BChannelBitmap - tcb.BChannelBitmap;
                                diff = cChan.GetBinary(BlueDiff);
#if DEBUG
                                s = new ThreeChannelBitmap(new OneChannelBitmap(diff.Width, diff.Height),
                                                               new OneChannelBitmap(diff.Width, diff.Height), cChan);
                                s.GetVisual().Save(@"C:\temp\bimg\img" + _calibrationStep + ".jpg");
#endif
                                break;
                        }
                        var topLeftCorner = GetTopLeftCorner(diff);
                        var topRightCorner = GetTopRightCorner(diff);
                        var bottomLeftCorner = GetBottomLeftCorner(diff);
                        var bottomRightCorner = GetBottomRightCorner(diff);
                        if (topLeftCorner.X < topRightCorner.X && topLeftCorner.X > bottomLeftCorner.X
                            && topRightCorner.Y < bottomRightCorner.Y && topRightCorner.Y < bottomLeftCorner.Y
                            && topLeftCorner.Y < bottomRightCorner.Y && bottomLeftCorner.X < bottomRightCorner.X
                            && topLeftCorner.X < bottomRightCorner.X && bottomLeftCorner.X < topRightCorner.X
                            && IsValid(topLeftCorner) && IsValid(topRightCorner) && IsValid(bottomLeftCorner) &&
                            IsValid(bottomRightCorner) && true)
                        {
                            Grid.AddPoint(_rects[j].TopLeft, topLeftCorner);
                            Grid.AddPoint(_rects[j].TopRight, topRightCorner);
                            Grid.AddPoint(_rects[j].BottomLeft, bottomLeftCorner);
                            Grid.AddPoint(_rects[j].BottomRight, bottomRightCorner);
                        }
                        else
                        {
                            _errors++;
                        }
                    }
                    _calibrationStep++;
                }
                    switch (_calibrationStep)
                    {
                        case 2:
                            var diff = (_blackImage - ThreeChannelBitmap.FromBitmap(e.NewImage)).GetGrayscale();
                            var b = diff.GetBinary(GreyDiff);
                            var s = new ThreeChannelBitmap(diff,
                                                               diff, diff);
                                s.GetVisual().Save(@"C:\temp\diffs\img" + _errors + ".jpg");
                            Grid.TopLeft = GetTopLeftCorner(b);
                            Grid.TopRight = GetTopRightCorner(b);
                            Grid.BottomLeft = GetBottomLeftCorner(b);
                            Grid.BottomRight = GetBottomRightCorner(b);
                            if (Grid.TopLeft.X < Grid.TopRight.X && Grid.TopLeft.X < Grid.BottomRight.X &&
                                Grid.BottomLeft.X < Grid.TopRight.X && Grid.BottomLeft.X < Grid.BottomRight.X &&
                                Grid.TopLeft.Y < Grid.BottomLeft.Y && Grid.TopLeft.Y < Grid.BottomRight.Y &&
                                Grid.TopRight.Y < Grid.BottomLeft.Y && Grid.TopRight.Y < Grid.BottomRight.Y &&
                                IsValid(Grid.TopLeft) && IsValid(Grid.TopRight) && IsValid(Grid.BottomLeft) &&
                                IsValid(Grid.BottomRight))
                            {
                                _calibrationStep++;
                                _vs.ClearRects();
                                FillRandomRects();
                                _sem = new SemaphoreSlim(3, 4);
                            }
                            else
                            {
                                _calibrationStep = 0;
                                _errors ++;
                            }
                            break;
                        case 1:
                            _blackImage = ThreeChannelBitmap.FromBitmap(e.NewImage);
                            _vs.AddRect(0, 0, _vs.Width, _vs.Height, DColor.FromArgb(255,255, 255, 255));
                            _calibrationStep++;
                            break;
                        case 0:
                            Grid = new Grid(e.NewImage.Width, e.NewImage.Height);
                            var thread = new Thread(() =>
                                {
                                    _vs.Show();
                                    //_vs.Draw();
                                });
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.Start();
                            thread.Join();
                            _vs.AddRect(0, 0, _vs.Width, _vs.Height, DColor.FromArgb(255, 0, 0, 0));
                            _calibrationStep++;
                            break;
                    }
            }
            await Task.Delay(500);
            Debug.WriteLine("releasing");
            _sem.Release();
        }

        private bool IsValid(Point point)
        {
            return point.X > 0 && point.Y > 0;
        }

        private void FillRandomRects()
        {
            var r = new Random();
            var tl = new Point(r.Next(_vs.Width-50), r.Next(_vs.Height-50));
            _rects[0] = new Rect(tl, new Point(r.Next((int)tl.X, _vs.Width-50)+50, r.Next((int)tl.Y, _vs.Height-50)+50));
            tl = new Point(r.Next(_vs.Width - 50), r.Next(_vs.Height - 50));
            _rects[1] = new Rect(tl, new Point(r.Next((int)tl.X, _vs.Width - 50) + 50, r.Next((int)tl.Y, _vs.Height - 50) + 50));
            tl = new Point(r.Next(_vs.Width - 50), r.Next(_vs.Height - 50));
            _rects[2] = new Rect(tl, new Point(r.Next((int)tl.X, _vs.Width - 50) + 50, r.Next((int)tl.Y, _vs.Height - 50) + 50));
            _vs.AddRect(new VPoint(_rects[0].TopLeft.X,_rects[0].Y), new VPoint(_rects[0].BottomRight.X, _rects[0].BottomRight.Y), DColor.FromArgb(255, 255, 0, 0));
            _vs.AddRect(new VPoint(_rects[1].TopLeft.X, _rects[1].Y), new VPoint(_rects[1].BottomRight.X, _rects[1].BottomRight.Y), DColor.FromArgb(255, 0, 255, 0));
            _vs.AddRect(new VPoint(_rects[2].TopLeft.X,_rects[2].Y), new VPoint(_rects[2].BottomRight.X, _rects[2].BottomRight.Y), DColor.FromArgb(255, 0, 0, 255));
            CheckIntersections();
        }

        private void CheckIntersections()
        {
            if (_rects[0].IntersectsWith(_rects[1]))
            {
                var rect = _rects[1];
                rect.Intersect(_rects[0]);
                _vs.AddRect(new VPoint(rect.TopLeft.X,rect.Y), new VPoint(rect.BottomRight.X, rect.BottomRight.Y), DColor.FromArgb(255, 255, 255, 0));
            }
            if (_rects[0].IntersectsWith(_rects[2]))
            {
                var rect = _rects[2];
                rect.Intersect(_rects[0]);
                _vs.AddRect(new VPoint(rect.TopLeft.X, rect.Y), new VPoint(rect.BottomRight.X, rect.BottomRight.Y), DColor.FromArgb(255, 255, 0, 255));
            }
            if (_rects[1].IntersectsWith(_rects[2]))
            {
                var rect = _rects[2];
                rect.Intersect(_rects[1]);
                _vs.AddRect(new VPoint(rect.TopLeft.X, rect.Y), new VPoint(rect.BottomRight.X, rect.BottomRight.Y), DColor.FromArgb(255, 0, 255, 255));
                if (rect.IntersectsWith(_rects[0]))
                {
                    rect.Intersect(_rects[0]);
                    _vs.AddRect(new VPoint(rect.TopLeft.X, rect.Y), new VPoint(rect.BottomRight.X, rect.BottomRight.Y), DColor.FromArgb(255, 255, 255, 255));
                }
            }
        }

        private Point GetBottomRightCorner(BinaryBitmap diff)
        {
            for (int i = diff.Width; i >= -diff.Height; i--) // it's wrong, but it's handled in Checkblock()
            {
                for (int j = 0; j <= diff.Height && j + i <= diff.Width; j++)
                {
                    if (CheckBlock(diff, i + j - Blocksize, diff.Height - j - Blocksize))
                    {
                        return new Point {X = i + j, Y = diff.Height - j};
                    }
                }
            }
            return new Point();
        }

        private Point GetBottomLeftCorner(BinaryBitmap diff)
        {
            for (int i = 0; i < diff.Width; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    if (CheckBlock(diff, i - j, diff.Height - j - Blocksize))
                    {
                        return new Point {X = i - j, Y = diff.Height - j};
                    }
                }
            }
            return new Point();
        }

        private Point GetTopRightCorner(BinaryBitmap diff)
        {
            for (int i = diff.Width - 1; i >= 0; i--)
            {
                for (int j = 0; j < diff.Width - i; j++)
                {
                    if (CheckBlock(diff, i + j - Blocksize, j))
                    {
                        return new Point {X = i + j, Y = j};
                    }
                }
            }
            return new Point();
        }

        private Point GetTopLeftCorner(BinaryBitmap diff)
        {
            for (int i = 0; i < diff.Width; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    if (CheckBlock(diff, i - j, j))
                    {
                        return new Point {X = i - j, Y = j};
                    }
                }
            }
            return new Point();
        }

        private bool CheckBlock(BinaryBitmap diff, int p, int j)
        {
            int sum = 0;
            if (p < 0 || j < 0)
            {
                return false;
            }
            for (int i = p; i <= p+Blocksize && i < diff.Width; i++)
            {
                for (int k = j; k <= j+Blocksize && k < diff.Height; k++)
                {
                    if (diff.Channel[i,k])
                    {
                        if (++sum>=Blockfill)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check if the grid needs to be recalibrated
        /// Check if something moved
        /// </summary>
        /// <returns>
        /// 0: ok
        /// 1: grid calibration needed
        /// 2: color calibration needed
        /// 3: both needed
        /// </returns>
        public int CheckCalibration()
        {
            // TODO dummy
            return 0;
        }

        /// <summary>
        /// Data for the matching
        /// </summary>
        public Grid Grid { get; private set; }

        /// <summary>
        /// Data from color calibration
        /// </summary>
        public Colorfilter ColorFilter { get; private set; }
    }
}
