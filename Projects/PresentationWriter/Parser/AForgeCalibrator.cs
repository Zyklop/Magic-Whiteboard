using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Linq;
using System.Windows;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using HSR.PresentationWriter.Parser.Events;
using WFVisuslizer;
using Point = System.Windows.Point;

namespace HSR.PresentationWriter.Parser
{
    class AForgeCalibrator
    {
        private CameraConnector _cc;
        private int _calibrationStep;
        private int _errors;
        private VisualizerControl _vs = VisualizerControl.GetVisualizer();
        private const int CalibrationFrames = 25;
        private Difference diffFilter = new Difference();
        private int _rowcount=1;
        private int _columncount=1;


        public AForgeCalibrator(CameraConnector cc)
        {
            _cc = cc;
            this.Grid = new Grid(0,0);
            //var thread = new Thread(() => _vs = new CalibratorWindow());
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
            //thread.Join();
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
            _cc.NewImage += BaseCalibration;
        }

        private void BaseCalibration(object sender, NewImageEventArgs e)
        {
            if (_errors > 100)
            {
                //calibration not possible
                return;
            }
            if (_calibrationStep == CalibrationFrames)
            {
                _cc.NewImage -= BaseCalibration;
                Grid.Calculate();
                _vs.Close();
            }
            else
            {
                Bitmap cChanBitmap;
                Bitmap diffBitmap = diffFilter.Apply(e.NewImage);
                if (_calibrationStep > 2)
                {
                    _vs.ClearRects();
                    FillRects();
                    var rectlist = new List<List<IntPoint>>();
                    for (int j = 0; j < 3; j++)
                    {
                        var cf = new ColorFiltering(new IntRange(0,50), new IntRange(0,50), new IntRange(0,50) );
                        switch (j)
                        {
                            case 0:
                                cf.Red = new IntRange(100,255);
                                break;
                            case 1:
                                cf.Green = new IntRange(100, 255);
                                break;
                            case 2:
                                cf.Blue = new IntRange(100, 255);
                                break;
                        }
                        cChanBitmap = cf.Apply(diffBitmap);
                        var blobCounter = new BlobCounter();
                        blobCounter.ObjectsOrder = ObjectsOrder.YX;
                        blobCounter.ProcessImage(cChanBitmap);
                        var blobs = blobCounter.GetObjectsInformation();
                        // add all detected rects
                        for (int i = 0, n = blobs.Length; i < n; i++)
                        {
                            List<IntPoint> corners = PointsCloud.FindQuadrilateralCorners(blobCounter.GetBlobsEdgePoints(blobs[i]));
                            rectlist.Add(corners);
                        }
                        //sort
                        foreach (var rect in rectlist)
                        {
                            InPlaceSort(rect);
                        }
                        InPlaceSort(rectlist);
                    }

                }
                else
                    switch (_calibrationStep)
                    {
                        case 2:
                        var thresholdFilter = new Threshold(50);
                        thresholdFilter.ApplyInPlace(diffBitmap);
                        var blobCounter = new BlobCounter {ObjectsOrder = ObjectsOrder.Size};
                            blobCounter.ProcessImage(diffBitmap);
                        var blobs = blobCounter.GetObjectsInformation();
                            List<IntPoint> corners = PointsCloud.FindQuadrilateralCorners(blobCounter.GetBlobsEdgePoints(blobs[0]));
                            InPlaceSort(corners);
                            Grid.TopLeft = new Point(corners[0].X,corners[0].Y);
                            Grid.TopRight = new Point(corners[1].X, corners[1].Y);
                            Grid.BottomLeft = new Point(corners[2].X, corners[2].Y);
                            Grid.BottomRight = new Point(corners[3].X, corners[3].Y);
                            if (Grid.TopLeft.X > 10 && Grid.TopRight.X < _vs.Width - 10 && Grid.BottomLeft.X > 10 &&
                                Grid.BottomRight.X < _vs.Width - 10 && Grid.TopLeft.Y > 10 && Grid.TopRight.Y > 10 && 
                                Grid.BottomLeft.Y < _vs.Height - 10 && Grid.BottomRight.Y < _vs.Height - 10 && 
                                Grid.TopLeft.X < Grid.BottomRight.X && 
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
                            diffFilter.OverlayImage = e.NewImage;
                            _vs.AddRect(0, 0, (int) _vs.Width, (int) _vs.Height, Color.FromArgb(255, 255, 255, 255));
                            _calibrationStep++;
                            break;
                        case 0:
                            Grid = new Grid(e.NewImage.Width, e.NewImage.Height);
                            var thread = new Thread(() =>
                                {
                                    _vs.Show();
                                    _vs.AddRect(0, 0, (int) _vs.Width, (int) _vs.Height, Color.FromArgb(255, 0, 0, 0));
                                });
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.Start();
                            thread.Join();
                            _calibrationStep++;
                            break;
                    }
            }
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
            for (int i = 0; i < _rowcount; i++)
            {
                var tmp2 = new List<List<IntPoint>>();
                tmp2.AddRange(rectlist.GetRange(0,_columncount));
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
            rect = rect.OrderBy(x => x.X).ToList();
            tmp = tmp.OrderBy(x => x.X).ToList();
            rect.AddRange(tmp);
        }

        private void FillRects()
        {
            int step = (_calibrationStep - 2)*10;
            for (int i = step; i < _vs.Width; i+=100)
            {
                
            }
        }

        public Grid Grid { get; set; }
    }
}
