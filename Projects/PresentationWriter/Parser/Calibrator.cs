using System;
using System.Collections.Generic;
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
        private ThreeChannelBitmap _lastImage;
        private CalibratorWindow _cw;

        public Calibrator(CameraConnector cc)
        {
            _cc = cc;
            Grid = new Grid();
            _cw = new CalibratorWindow();
            Calibrate();
            CalibrateColors();
        }

        public void CalibrateColors()
        {
        }

        public void Calibrate()
        {
            _cc.NewImage += BaseCalibration;
        }

        private void BaseCalibration(object sender, Events.NewImageEventArgs e)
        {
            if (_lastImage == null)
            {
                _lastImage = e.NewImage;
            }
            else
            {
                var diff = (_lastImage - e.NewImage).GetGrayscale();
                SetTopLeftCorner(diff);
                SetTopRightCorner(diff);
                SetBottomLeftCorner(diff);
                SetBottomRightCorner(diff);
                _cc.NewImage -= BaseCalibration;
            }
        }

        private void SetBottomRightCorner(OneChannelBitmap diff)
        {
            for (int i = diff.Width; i > diff.Width - diff.Height; i--)
            {
                for (int j = 1; j <= i; j++)
                {
                    if (CheckBlock(diff, i + j - Blocksize, diff.Height - j - Blocksize))
                    {
                        Grid.BottomRight = new Point {X = i + j, Y = diff.Height - j};
                        break;
                    }
                }
            }
        }

        private void SetBottomLeftCorner(OneChannelBitmap diff)
        {
            for (int i = 0; i < diff.Height; i++)
            {
                for (int j = 1; j <= i; j++)
                {
                    if (CheckBlock(diff, i - j, diff.Height - j - Blocksize))
                    {
                        Grid.BotttomLeft = new Point {X = i - j, Y = diff.Height - j};
                        break;
                    }
                }
            }
        }

        private void SetTopRightCorner(OneChannelBitmap diff)
        {
            for (int i = diff.Width - 1; i > diff.Width - diff.Height; i--)
            {
                for (int j = 0; j < diff.Width - i; j++)
                {
                    if (CheckBlock(diff, i + j - Blocksize, j))
                    {
                        Grid.TopRight = new Point {X = i + j, Y = j};
                        return;
                    }
                }
            }
        }

        private void SetTopLeftCorner(OneChannelBitmap diff)
        {
            for (int i = 0; i < diff.Height; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    if (CheckBlock(diff, i - j, j))
                    {
                        Grid.TopLeft = new Point {X = i - j, Y = j};
                        return;
                    }
                }
            }
        }

        private bool CheckBlock(OneChannelBitmap diff, int p, int j)
        {
            int sum = 0;
            for (int i = p; i <= p+Blocksize && i < diff.Width; i++)
            {
                for (int k = j; k <= j+Blocksize && k < diff.Height; k++)
                {
                    if (diff.Channel[p,j]>=GreyDiff)
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
