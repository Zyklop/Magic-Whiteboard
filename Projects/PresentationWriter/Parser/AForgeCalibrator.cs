using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using HSR.PresWriter.IO.Events;
using HSR.PresWriter.PenTracking.Events; // TODO: wieso?
using HSR.PresWriter.IO;
using WFVisuslizer;
using Point = System.Drawing.Point;

namespace HSR.PresWriter.PenTracking
{
    enum Channels
    {
        Red,
        Green,
        Blue,
        GreenAndBlue
    }

    class AForgeCalibrator: ICalibrator
    {
        private IPictureProvider _cc;
        private int _calibrationStep;
        private int _errors;
        private VisualizerControl _vs;
        private const int CalibrationFrames = 28; //must be n^2+3
        private Difference diffFilter = new Difference();
        private const int Rowcount=20;
        private const int Columncount=15;
        private SemaphoreSlim _sem;
        private Task t;
        private double sqrheight;
        private double sqrwidth;
        private const int MeanDiff = 5;
        private const int ColorDiff = 8;


        public AForgeCalibrator(IPictureProvider provider)
        {
            _cc = provider;
            this.Grid = new Grid(0,0);
            //var thread = new Thread(() => _vs = new CalibratorWindow());
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
            //thread.Join();
        }

        // TODO Calibrate Funktionen aufräumen
        public void StartCalibration()
        {
            _vs = VisualizerControl.GetVisualizer();
            Calibrate();
            CalibrateColors();
        }


        public void CalibrateColors()
        {
        }

        public void Calibrate()
        {

            _calibrationStep = 0;
            _errors = 0;
            sqrwidth = ((double)_vs.Width) / Rowcount;
            sqrheight = ((double)_vs.Height) / Columncount;
            _sem = new SemaphoreSlim(1, 1);
            _cc.FrameReady += BaseCalibration; // TODO
        }

        public int CheckCalibration()
        {
            return 0;
        }

        private void BaseCalibration(object sender, FrameReadyEventArgs e)
        {
            if (t != null && t.Exception != null)
            {
                throw t.Exception;
            }
            if (_sem.CurrentCount >= 1)//_t.Status != TaskStatus.Running)
            {
                _sem.Wait();
                Task.Factory.StartNew(() => t = CalibThread(e));
            }
        }


        private async Task CalibThread(FrameReadyEventArgs e)
        {
            Debug.WriteLine("Calibrating " + _calibrationStep);
            e.Frame.Bitmap.Save(@"C:\temp\aforge\src\img" + _calibrationStep + ".jpg");
            //var stats = new ImageStatistics(e.Frame.Bitmap);
            //        //var histogram = stats.Gray;
            ////Debug.WriteLine("Grey: Min: " + histogram.Min + " Max: " + histogram.Max + " Mean: " + histogram.Mean +
            ////    " Dev: " + histogram.StdDev + " Median: " + histogram.Median);
            //var histogram = stats.Green;
            //Debug.WriteLine("Green: Min: " + histogram.Min + " Max: " + histogram.Max + " Mean: " + histogram.Mean +
            //    " Dev: " + histogram.StdDev + " Median: " + histogram.Median);
            //histogram = stats.Blue;
            //Debug.WriteLine("Blue: Min: " + histogram.Min + " Max: " + histogram.Max + " Mean: " + histogram.Mean +
            //    " Dev: " + histogram.StdDev + " Median: " + histogram.Median);
            //histogram = stats.Red;
            //Debug.WriteLine("Red: Min: " + histogram.Min + " Max: " + histogram.Max + " Mean: " + histogram.Mean +
            //e.Frame.Bitmap.Save(@"C:\temp\aforge\src\img" + _calibrationStep + ".jpg");
            //    " Dev: " + histogram.StdDev + " Median: " + histogram.Median);
            //e.NewImage.Save(@"C:\temp\aforge\src\img" + _calibrationStep + ".jpg");
            if (_errors > 100)
            {
                //calibration not possible
                return;
            }
            if (_calibrationStep == 3)
            {
                _cc.FrameReady -= BaseCalibration; // TODO
                //Grid.Calculate();
                _vs.Close();
                CalibrationCompleted(this, new EventArgs());
            }
            else
            {
                //var diffBitmap = new Bitmap(1,1);
                //if(diffFilter.OverlayImage != null)
                //    diffBitmap = diffFilter.Apply(e.NewImage);
                //diffBitmap.Save(@"C:\temp\aforge\diff\img" + _calibrationStep + ".jpg");
                if (_calibrationStep > 2)
                {
                    _vs.ClearRects();
                    FillRects();
                    //var gf = new ColorFiltering(new IntRange(0, 255), //(int) stats.Red.Mean), 
                    //                            new IntRange((int) (stats.Green.Mean + ColorDiff), 255),
                    //                            new IntRange(0, 255));//(int) stats.Blue.Mean));
                    //var bf = new ColorFiltering(new IntRange(0, 255),//(int)stats.Red.Mean), 
                    //    new IntRange(0, 255),//(int) stats.Green.Mean), 
                    //    new IntRange((int) stats.Blue.Mean + ColorDiff, 255));
                    var gbm = PartiallyApplyAvgFilter(e.Frame.Bitmap, Channels.Green, 8, 8, 8);
                    gbm.Save(@"C:\temp\aforge\gimg\img" + _calibrationStep + ".jpg");
                    var bbm = PartiallyApplyAvgFilter(e.Frame.Bitmap, Channels.Blue, 8, 8, 8);
                    bbm.Save(@"C:\temp\aforge\bimg\img" + _calibrationStep + ".jpg");
                    var gblobCounter = new BlobCounter {ObjectsOrder = ObjectsOrder.YX, 
                        MaxHeight = 30, MinHeight = 15, MaxWidth = 30, MinWidth = 15, FilterBlobs = true, CoupledSizeFiltering = false};
                    gblobCounter.ProcessImage(gbm);
                    var bblobCounter = new BlobCounter { ObjectsOrder = ObjectsOrder.YX, 
                        MaxHeight = 30, MinHeight = 15, MaxWidth = 30, MinWidth = 15, FilterBlobs = true, CoupledSizeFiltering = false};
                    bblobCounter.ProcessImage(bbm);
                    ProcessBlobs(gblobCounter, 0);
                    ProcessBlobs(bblobCounter, 1);
                    _calibrationStep++;
                }
                else
                    switch (_calibrationStep)
                    {
                        case 2:
                            //var thresholdFilter = new Threshold(50);
                            //thresholdFilter.ApplyInPlace(diffBitmap);
                            //var cf = new ColorFiltering(new IntRange(0, 255), //red is bad
                            //                            new IntRange((int) stats.Green.Mean + MeanDiff, 255),
                            //                            new IntRange((int) stats.Blue.Mean + MeanDiff, 255));
                            //var bm = cf.Apply(e.NewImage);
                            //var bm = PartiallyApplyAvgFilter(e.Frame.Bitmap, Channels.GreenAndBlue, 2, 2, MeanDiff);
                            var bm = UnmanagedImage.FromManagedImage(e.Frame.Bitmap);
                            bm = diffFilter.Apply(bm);
                            var gf = new GaussianBlur(9.0,3);
                            gf.ApplyInPlace(bm);
                            var cf = new ColorFiltering(new IntRange(10, 255), new IntRange(20, 255),
                                                        new IntRange(20, 255));
                            cf.ApplyInPlace(bm);
                            var blobCounter = new BlobCounter {ObjectsOrder = ObjectsOrder.Size, BackgroundThreshold = Color.FromArgb(255,15,20,20), FilterBlobs = true};
                            blobCounter.ProcessImage(bm);
                            bm.ToManagedImage().Save(@"C:\temp\aforge\diff.jpg");
                            var blobs = blobCounter.GetObjectsInformation();
                            int i = 0;
                            List<IntPoint> corners;
                            do
                            {
                                corners =
                                    PointsCloud.FindQuadrilateralCorners(blobCounter.GetBlobsEdgePoints(blobs[i++]));
                            } while (corners.Count != 4);
                            InPlaceSort(corners);
                            Grid.TopLeft = new Point(corners[0].X, corners[0].Y);
                            Grid.TopRight = new Point(corners[1].X, corners[1].Y);
                            Grid.BottomLeft = new Point(corners[2].X, corners[2].Y);
                            Grid.BottomRight = new Point(corners[3].X, corners[3].Y);
                            if (Grid.TopLeft.X > 10 && Grid.TopRight.X < _vs.Width - 10 && Grid.BottomLeft.X > 10 &&
                                Grid.BottomRight.X < _vs.Width - 10 && Grid.TopLeft.Y > 10 && Grid.TopRight.Y > 10 &&
                                Grid.BottomLeft.Y < _vs.Height - 10 && Grid.BottomRight.Y < _vs.Height - 10 &&
                                Grid.TopLeft.X < Grid.BottomRight.X && blobs[i-1].Area > 60000 &&
                                Grid.BottomLeft.X < Grid.TopRight.X && Grid.BottomLeft.X < Grid.BottomRight.X &&
                                Grid.TopLeft.Y < Grid.BottomLeft.Y && Grid.TopLeft.Y < Grid.BottomRight.Y &&
                                Grid.TopRight.Y < Grid.BottomLeft.Y && Grid.TopRight.Y < Grid.BottomRight.Y)
                            {
                                _calibrationStep++;
                                _vs.ClearRects();
                                FillRects();
                                Grid.AddPoint(new Point(), new Point(corners[0].X, corners[0].Y));
                                Grid.AddPoint(new Point(0, _vs.Height), new Point(corners[1].X, corners[1].Y));
                                Grid.AddPoint(new Point(_vs.Width, 0), new Point(corners[2].X, corners[2].Y));
                                Grid.AddPoint(new Point(_vs.Width, _vs.Height), new Point(corners[3].X, corners[3].Y));
                            }
                            else
                            {
                                _calibrationStep = 0;
                                _errors++;
                            }
                            break;
                        case 1:
                            diffFilter.OverlayImage = e.Frame.Bitmap;
                            //_vs.AddRect(0, 0, (int) _vs.Width, (int) _vs.Height, Color.FromArgb(255, 255, 255, 255));
                            _vs.ClearRects();
                            for (int y = 0; y < Columncount; y++)
                            {
                                for (int x = 0; x < Rowcount; x++)
                                {
                                    if (!(y % 2 == 0 && x % 2 == 0 || y % 2 == 1 && x % 2 == 1))
                                    {
                                        _vs.AddRect((int)(x * sqrwidth), (int)(y * sqrheight),
                                            (int)sqrwidth, (int)sqrheight,
                                        Color.FromArgb(255, 255, 255, 255));
                                    }
                                }
                            }
                            _vs.Draw();
                            _calibrationStep++;
                            break;
                        case 0:
                            Grid = new Grid(e.Frame.Bitmap.Width, e.Frame.Bitmap.Height);
                            var thread = new Thread(() =>
                                {
                                    _vs.Show();
                                    _vs.AddRect(0, 0, (int) _vs.Width, (int) _vs.Height, Color.FromArgb(255, 0, 0, 0));
                                });
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.Start();
                            thread.Join();
                            for (int y = 0; y < Columncount; y++)
                            {
                                for (int x = 0; x < Rowcount; x++)
                                {
                                    if (y % 2 == 0 && x % 2 == 0 || y % 2 == 1 && x % 2 == 1)
                                    {
                                        _vs.AddRect((int)(x * sqrwidth), (int)(y * sqrheight),
                                            (int)sqrwidth, (int)sqrheight,
                                        Color.FromArgb(255, 255, 255, 255));
                                    }
                                }
                            }
                            _vs.Draw();
                            _calibrationStep++;
                            break;
                    }
            }
            Debug.WriteLine("Releasing");
            await Task.Delay(100);
            _sem.Release();
        }

        private Bitmap PartiallyApplyAvgFilter(Bitmap src, Channels channel, int x, int y, int diff)
        {
            var parts = new UnmanagedImage[x,y];
            var res = new Bitmap(src.Width, src.Height);
            int width = src.Width/x;
            int height = src.Height/y;
            using (var g = Graphics.FromImage(res))
            {
                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        //var crop = src.Clone(new Rectangle(i*width, j*height, width, height), src.PixelFormat)
                        UnmanagedImage crop;
                        BitmapData imageData = src.LockBits(
                            new Rectangle(i*width, j*height, width, height),
                            ImageLockMode.ReadWrite, src.PixelFormat);

                        try
                        {
                            crop = new UnmanagedImage(imageData);
                            // apply several routines to the unmanaged image
                        }
                        finally
                        {
                            src.UnlockBits(imageData);
                        }

                        var s = new ImageStatistics(crop);
                        var cf = new ColorFiltering(new IntRange(0, 255), new IntRange(0, 255), new IntRange(0, 255));
                        switch (channel)
                        {
                            case Channels.Red:
                                cf.Red = new IntRange((int) s.Red.Mean + diff, 255);
                                break;
                            case Channels.Green:
                                cf.Green = new IntRange((int) s.Green.Mean + diff, 255);
                                cf.Blue = new IntRange(0, (int) s.Blue.Mean + 10);
                                break;
                            case Channels.Blue:
                                cf.Blue = new IntRange((int) s.Blue.Mean + diff, 255);
                                cf.Green = new IntRange(0, (int) s.Green.Mean + 10);
                                break;
                            case Channels.GreenAndBlue:
                                cf.Green = new IntRange((int) s.Green.Mean + diff, 255);
                                cf.Blue = new IntRange((int) s.Blue.Mean + diff, 255);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("channel");
                        }
                        cf.ApplyInPlace(crop);
                        g.DrawImageUnscaled(crop.ToManagedImage(), i*width, j*height);
                    }
                }
                g.Flush();
            }
            return res;
        }

        private void ProcessBlobs(BlobCounter blobCounter, int offset)
        {
            blobCounter.FilterBlobs = true;
            var blobs = blobCounter.GetObjectsInformation().ToList();
            // remove when outside
            blobs.RemoveAll(x => !Grid.Contains(x.CenterOfGravity));
            //find topleft
            //var top = blobs.Where(x => x.CenterOfGravity.X == blobs.GetRange(0,Rowcount).
            //    Min(y => y.CenterOfGravity.X)).
            //    OrderBy(z=>z.CenterOfGravity.Y).First();
            var top = blobs.Where(x => IsNextTo(x.CenterOfGravity, Grid.TopLeft, 0, 0, 1.5 + offset)).OrderBy(x => x.Area).ToList();
            if (top.Any() && IsNextTo(top.First().CenterOfGravity, Grid.TopLeft,0,0,1.5 + offset))
            {
                blobs.Remove(top.Last());
                if (ProcessBlobs(top.Last(), blobs, 0, 0, offset, blobCounter) < Rowcount*Columncount*0.75)
                    _errors++;
            }
            else
            {
                _errors++;
                return;
            }
        }

        private int ProcessBlobs(Blob blob, List<Blob> rest, int x, int y, int offset, BlobCounter blobCounter)
        {
            if (x < 0 || y < 0 || y > Columncount || x > Rowcount)
            {
                //throw new ArgumentException("X: "+x+ " Y: " + y + " isn't a valid blob position");
                Debug.WriteLine("X: " + x + " Y: " + y + " isn't a valid blob position");
                return 0;
            }
            var corners =
                PointsCloud.FindQuadrilateralCorners(blobCounter.GetBlobsEdgePoints(blob));
            if (corners.Count == 4)
            {
                InPlaceSort(corners);
                double xOff = ((_calibrationStep - 3)%(int) Math.Sqrt(CalibrationFrames - 3))*sqrwidth/
                              (int) Math.Sqrt(CalibrationFrames - 3);
                double yOff = Math.Floor((_calibrationStep - 3)/Math.Sqrt(CalibrationFrames - 3))*sqrwidth/
                              (int) Math.Sqrt(CalibrationFrames - 3);
                Grid.AddPoint((int) (x*sqrwidth + xOff), (int) (y*sqrheight + yOff), corners[0].X, corners[0].Y);
                Grid.AddPoint((int) ((x + 1)*sqrwidth + xOff), (int) (y*sqrheight + yOff), corners[1].X, corners[1].Y);
                Grid.AddPoint((int) (x*sqrwidth + xOff), (int) ((y + 1)*sqrheight + yOff), corners[2].X, corners[2].Y);
                Grid.AddPoint((int) ((x + 1)*sqrwidth + xOff), (int) ((y + 1)*sqrheight + yOff), corners[3].X,
                              corners[3].Y);
            }
            else
            {
                _errors++;
            }
            //find neighbours
            int maxNeigbours = 4;
            if (x == 0 || x == Rowcount)
                maxNeigbours--;
            if (y == 0 || y == Columncount)
                maxNeigbours--;
            var n = rest.Where(m => IsNextTo(blob.CenterOfGravity, m.CenterOfGravity, x, y, 2.5)).ToList();
            if (n.Count() > maxNeigbours)
                Debug.WriteLine("too many neighbours");
                //throw new InvalidOperationException("Too much Blobs");
            int res = n.Count();
            foreach (var b in n)
            {
                var xdiff = b.CenterOfGravity.X - blob.CenterOfGravity.X;
                var ydiff = b.CenterOfGravity.Y - blob.CenterOfGravity.Y;
                rest.Remove(b);
                if (rest.Any())
                {
                    if (xdiff > ydiff)
                    {
                        if (xdiff > 0)
                        {
                            res += ProcessBlobs(b, rest, x + 1, y, offset, blobCounter);
                        }
                        else
                        {
                            res += ProcessBlobs(b, rest, x - 1, y, offset, blobCounter);
                        }
                    }
                    else
                    {
                        if (ydiff > 0)
                        {
                            res += ProcessBlobs(b, rest, x, y + 1, offset, blobCounter);
                        }
                        else
                        {
                            res += ProcessBlobs(b, rest, x, y - 1, offset, blobCounter);
                        }
                    }
                }
            }
            return res;
        }

        private bool IsNextTo(AForge.Point p1, AForge.Point p2, int rownr, int colnr, double maxDist)
        {
            return IsNextTo(p1, new Point((int) p2.X, (int) p2.Y), rownr, colnr, maxDist);
        }

        private bool IsNextTo(AForge.Point p1, Point p2, int rownr, int colnr, double maxDist)
        {
            return Math.Abs(p1.X - p2.X) < PredictRowDistance(colnr)*maxDist &&
                   Math.Abs(p1.Y - p2.Y) < PredictColumnDistance(rownr)*maxDist;
        }

        private int PredictRowDistance(int columnnr)
        {
            var dist = (Grid.TopRight.X - Grid.TopLeft.X)*(1.0 - (double) columnnr/Columncount) +
                       (Grid.BottomRight.X - Grid.BottomLeft.X)*((double) columnnr/Columncount);
            return (int) Math.Round(dist/Rowcount);
        }

        private int PredictColumnDistance(int rownr)
        {

            var dist = (Grid.BottomLeft.Y - Grid.TopLeft.Y)*(1.0 - (double) rownr/Rowcount) +
                       (Grid.BottomRight.Y - Grid.TopRight.Y)*((double) rownr/Rowcount);
            return (int)Math.Round(dist / Columncount);
        }

        /// <summary>
        /// Sorting the whole list
        /// </summary>
        /// <remarks>Rects have to be sorted</remarks>
        /// <param name="rectlist"></param>
        private void InPlaceSort(List<List<IntPoint>> rectlist)
        {
            var tmp = new List<List<IntPoint>>();
            rectlist = rectlist.OrderBy(x => x.First().Y).ToList();
            for (int i = 0; i < Rowcount; i++)
            {
                var tmp2 = new List<List<IntPoint>>();
                tmp2.AddRange(rectlist.GetRange(0,Columncount));
            }
        }

        private void InPlaceSort(List<IntPoint> rect)
        {
            if (rect == null || rect.Count != 4)
                throw new ArgumentException("Not a rectancle");
            var tmp = new List<IntPoint>(rect);
            rect.Clear();
            var tl = tmp.First(x => x.Y == tmp.Min(y => y.Y));
            tmp.Remove(tl);
            rect.Add(tl);
            tl = tmp.First(x => x.Y == tmp.Min(y => y.Y));
            tmp.Remove(tl);
            rect.Add(tl);
            rect.Sort((x,y) => x.X.CompareTo(y.X));
            tmp = tmp.OrderBy(x => x.X).ToList();
            //rect.AddRange(tmp);
            rect.Add(tmp[0]);
            rect.Add(tmp[1]);
        }

        private void FillRects()
        {

            double xOff = ((_calibrationStep - 3) % (int)Math.Sqrt(CalibrationFrames - 3)) * 
                sqrwidth / Math.Sqrt(CalibrationFrames - 3);
            double yOff = Math.Floor((_calibrationStep - 3) / Math.Sqrt(CalibrationFrames - 3)) * 
                sqrwidth / Math.Sqrt(CalibrationFrames - 3);
            for (int y = 0; y < Columncount; y++)
            {
                for (int x = 0; x < Rowcount; x++)
                {
                    if (y%2==0 && x%2 == 0)
                    {
                        _vs.AddRect((int)(x * sqrwidth + xOff), (int)(y * sqrheight + yOff), 
                            (int)sqrwidth, (int)sqrheight,
                        Color.FromArgb(255,0,255,0)); 
                    }
                    if (y % 2 == 1 && x % 2 == 1)
                    {
                        _vs.AddRect((int)(x * sqrwidth + xOff), (int)(y * sqrheight + yOff),
                            (int)sqrwidth, (int)sqrheight,
                        Color.FromArgb(255, 0, 0, 255));
                    }
                }
            }
            _vs.Draw();
        }

        public Grid Grid { get; set; }
        public Colorfilter ColorFilter { get; private set; }


        public event EventHandler CalibrationCompleted;
    }
}
