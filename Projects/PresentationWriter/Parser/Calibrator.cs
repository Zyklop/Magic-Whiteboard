using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR.PresentationWriter.Parser.Images;
using ImageVisualizer;

namespace HSR.PresentationWriter.Parser
{
    internal class Calibrator
    {
        private CameraConnector _cc;
        private const byte GreyDiff = 20;
        private const int Blocksize = 10;
        private const int Blockfill = 80; // Number of pixels needed for a Block to be valid. Depends on Blocksize.
        private const int CalibrationFrames = 300; // Number of Frames used for calibration. Divide by 10 to get Time for calibration.
        private ThreeChannelBitmap _blackImage;
        private CalibratorWindow _cw;
        private int _calibrationStep;
        private int _errors = 0;
        private Rect[] rects = new Rect[3];

        public Calibrator(CameraConnector cc)
        {
            _cc = cc;
            _cw = new CalibratorWindow();
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

        private void BaseCalibration(object sender, Events.NewImageEventArgs e)
        {
            if (_errors > 100)
            {
                //calibration not possible
                return;
            }
            if (_calibrationStep == CalibrationFrames || !Grid.DataNeeded)
            {
                _cc.NewImage -= BaseCalibration;
                Grid.Calculate();
                _cw.Close();
            }
            else if (_calibrationStep > CalibrationFrames/2)
            {
                
            }
            else if (_calibrationStep > 2)
            {
                for (int j = 0; j < 3; j++)
                {
                    OneChannelBitmap diff;
                    switch (j)
                    {
                        case 0:
                            diff = _blackImage.RChannelBitmap - e.NewImage.RChannelBitmap;
                            break;
                        case 1:
                            diff = _blackImage.GChannelBitmap - e.NewImage.GChannelBitmap;
                            break;
                        case 2:
                            diff = _blackImage.GChannelBitmap - e.NewImage.GChannelBitmap;
                            break;
                    }

                }
            }
            else switch (_calibrationStep)
            {
                case 2:
                    var diff = (_blackImage - e.NewImage).GetGrayscale();
                    Grid.TopLeft = SetTopLeftCorner(diff);
                    Grid.TopRight = SetTopRightCorner(diff);
                    Grid.BottomLeft = SetBottomLeftCorner(diff);
                    Grid.BottomRight = SetBottomRightCorner(diff);
                    if (Grid.TopLeft.X < Grid.TopRight.X && Grid.TopLeft.X < Grid.BottomRight.X &&
                        Grid.BottomLeft.X < Grid.TopRight.X && Grid.BottomLeft.X < Grid.BottomRight.X &&
                        Grid.TopLeft.Y < Grid.BottomLeft.Y && Grid.TopLeft.Y < Grid.BottomRight.Y &&
                        Grid.TopRight.Y < Grid.BottomLeft.Y && Grid.TopRight.Y < Grid.BottomRight.Y)
                    {
                        _calibrationStep++;
                        _cw.ClearRects();
                        FillRandomRects();
                    }
                    else
                    {
                        _calibrationStep = 0;
                        _errors ++;
                    }
                    break;
                case 1:
                    _blackImage = e.NewImage;
                    _cw.AddRect(0,0,(int) _cw.Width,(int) _cw.Height, Color.FromRgb(255,255,255));
                    _calibrationStep++;
                    break;
                case 0:
                    Grid = new Grid(e.NewImage.Width,e.NewImage.Height);
                    _cw.AddRect(0,0,(int) _cw.Width,(int) _cw.Height, Color.FromRgb(0,0,0));
                    _cw.Show();
                    _calibrationStep++;
                    break;
            }
        }

        private void FillRandomRects()
        {
            var r = new Random();
            var tl = new Point(r.Next((int) _cw.Width), r.Next((int) _cw.Height));
            rects[0] = new Rect(tl, new Point(r.Next((int) tl.X,(int) _cw.Width), r.Next((int) tl.Y, (int)_cw.Height)));
            tl = new Point(r.Next((int)_cw.Width), r.Next((int)_cw.Height));
            rects[1] = new Rect(tl, new Point(r.Next((int) tl.X, (int)_cw.Width), r.Next((int) tl.Y, (int)_cw.Height)));
            tl = new Point(r.Next((int)_cw.Width), r.Next((int)_cw.Height));
            rects[2] = new Rect(tl, new Point(r.Next((int) tl.X, (int)_cw.Width), r.Next((int) tl.Y, (int)_cw.Height)));
            _cw.AddRect(rects[0].TopLeft, rects[0].BottomRight, Color.FromRgb(255, 0, 0));
            _cw.AddRect(rects[1].TopLeft, rects[1].BottomRight, Color.FromRgb(0, 255, 0));
            _cw.AddRect(rects[2].TopLeft, rects[2].BottomRight, Color.FromRgb(0, 0, 255));
        }

        private Point SetBottomRightCorner(OneChannelBitmap diff)
        {
            for (int i = diff.Width; i >= 0; i--)
            {
                for (int j = 0; j <= i; j++)
                {
                    if (CheckBlock(diff, i + j - Blocksize, diff.Height - j - Blocksize))
                    {
                        return new Point {X = i + j, Y = diff.Height - j};
                    }
                }
            }
            return new Point();
        }

        private Point SetBottomLeftCorner(OneChannelBitmap diff)
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

        private Point SetTopRightCorner(OneChannelBitmap diff)
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

        private Point SetTopLeftCorner(OneChannelBitmap diff)
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
            return new Point(-1, -1);
        }

        private bool CheckBlock(OneChannelBitmap diff, int p, int j)
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
                    if (diff.Channel[i,k]>=GreyDiff)
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

        public int CheckCalibration()
        {
            // TODO dummy
            return 0;
        }

        public Grid Grid { get; private set; }

        public Colorfilter ColorFilter { get; private set; }
    }
}
