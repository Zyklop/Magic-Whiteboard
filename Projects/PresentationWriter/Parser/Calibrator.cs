using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR.PresentationWriter.Parser.Images;

namespace HSR.PresentationWriter.Parser
{
    internal class Calibrator
    {
        private CameraConnector _cc;
        private const byte GreyDiff = 20;
        private const int Blocksize = 10;
        private const int Blockfill = 80; // Number of pixels needed for a Block to be valid. Depends on Blocksize.
        private ThreeChannelBitmap _lastImage;

        public Calibrator(CameraConnector cc)
        {
            _cc = cc;
            Grid = new Grid();
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
                //top left corner
                var b = false;
                for (int i = 0; i < diff.Height; i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        if (CheckBlock(diff,i-j,j))
                        {
                            b = true;
                            Grid.TopLeft = new Point{X = i-j, Y = j};
                            break;
                        }
                    }
                    if (b) break;
                }
                //top right corner
                b = false;
                for (int i = diff.Width-1; i > diff.Width - diff.Height; i--)
                {
                    for (int j = 0; j < diff.Width-i; j++)
                    {
                        if (CheckBlock(diff, i + j - Blocksize, j))
                        {
                            b = true;
                            Grid.TopRight = new Point{X = i+j, Y = j};
                            break;
                        }
                    }
                    if (b) break;
                }
                //bottom left corner
                b = false;
                for (int i = 0; i < diff.Height; i++)
                {
                    for (int j = 1; j <= i; j++)
                    {
                        if (CheckBlock(diff, i - j, diff.Height - j - Blocksize))
                        {
                            b = true;
                            Grid.BotttomLeft = new Point { X = i - j, Y = diff.Height - j };
                            break;
                        }
                    }
                    if (b) break;
                }
                //bottom right corner
                b = false;
                for (int i = diff.Width; i > diff.Width - diff.Height; i--)
                {
                    for (int j = 1; j <= i; j++)
                    {
                        if (CheckBlock(diff, i + j - Blocksize, diff.Height - j - Blocksize))
                        {
                            b = true;
                            Grid.BottomRight = new Point { X = i + j, Y = diff.Height - j };
                            break;
                        }
                    }
                    if (b) break;
                }
                if (b)
                {
                    _cc.NewImage -= BaseCalibration;
                }
            }
        }

        private bool CheckBlock(OneChannelBitmap diff, int p, int j)
        {
            int sum = 0;
            for (int i = p; i <= p+Blocksize; i++)
            {
                for (int k = j; k <= j+Blocksize; k++)
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
