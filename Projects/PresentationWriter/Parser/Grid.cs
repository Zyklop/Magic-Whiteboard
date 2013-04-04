using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;

namespace HSR.PresWriter.PenTracking
{
    public class Grid
    {
        private List<Point>[,] _calibratorData;
        private Point[,] _mapData;
        private bool _needed = true;
        private Point _topLeft;
        private Point _topRight;
        private Point _bottomLeft;
        private Point _bottomRight;
        private SortedDictionary<int, SortedSet<int>> _refPoints; 

        /// <summary>
        /// Creating a mapping grid
        /// </summary>
        /// <param name="width">width of the source image</param>
        /// <param name="height">height of the source image</param>
        public Grid(int width, int height)
        {
            _mapData = new Point[width,height];
            _calibratorData = new List<Point>[width,height];
            _refPoints = new SortedDictionary<int, SortedSet<int>>();
        }

        /// <summary>
        /// Maximum width of the matching area
        /// </summary>
        public int Width { get
        {
            return (int) (new List<double>
                {
                    TopRight.X - TopLeft.X, 
                    BottomRight.X - BottomLeft.X, 
                    TopRight.X - BottomLeft.X, 
                    BottomRight.X - TopLeft.X
                }).Max();
        } }

        /// <summary>
        /// Maximum height of the matching area
        /// </summary>
        public int Height
        {
            get
            {
                return (int)(new List<double>
                {
                    BottomRight.Y - TopLeft.Y, 
                    BottomRight.Y - TopLeft.Y, 
                    BottomLeft.Y - TopRight.Y, 
                    BottomLeft.Y - TopLeft.Y
                }).Max();
            }
        }

        /// <summary>
        /// More data needed for Calibration
        /// </summary>
        public bool DataNeeded { get { return _needed; } }

        /// <summary>
        /// A good area for calibration
        /// </summary>
        /// <returns></returns>
        public Rect Needed()
        {
            return new Rect();
        }

        /// <summary>
        /// Top left corner
        /// </summary>
        public Point TopLeft
        {
            get { return _topLeft; }
            set { 
                _topLeft = value;
            }
        }

        /// <summary>
        /// Top right corner
        /// </summary>
        public Point TopRight
        {
            get { return _topRight; }
            set
            {
                _topRight = value;
            }
        }

        /// <summary>
        /// Bottom left corner
        /// </summary>
        public Point BottomLeft
        {
            get { return _bottomLeft; }
            set
            {
                _bottomLeft = value;
            }
        }

        /// <summary>
        /// Bottom left corner
        /// </summary>
        public Point BottomRight
        {
            get { return _bottomRight; }
            set
            {
                _bottomRight = value;
            }
        }

        public bool Contains(Point p)
        {
            //var at = Angle(TopLeft, p, TopRight);
            //var al = Angle(TopLeft, p, BottomLeft);
            //var ar = Angle(TopRight, p, BottomRight);
            //var ab = Angle(BottomLeft, p, BottomRight);
            //return at > 180 && al < 180 && ar < 180 && ab < 180;
            // internal method
            var gp = new GraphicsPath();
            gp.AddLine((int) TopLeft.X, (int) TopLeft.Y, (int)TopRight.X, (int)TopRight.Y);
            gp.AddLine((int)TopRight.X, (int)TopRight.Y, (int)BottomRight.X, (int)BottomRight.Y);
            gp.AddLine((int)BottomRight.X, (int)BottomRight.Y, (int)BottomLeft.X, (int)BottomLeft.Y);
            gp.AddLine((int)BottomLeft.X, (int)BottomLeft.Y, (int)TopLeft.X, (int)TopLeft.Y);
            return gp.IsVisible((int) p.X,(int) p.Y);
        }

        //private double Azimuth(Point start, Point finish)
        //{
        //    var a = new Vector(p.X - left.X, p.Y - left.Y);
        //    var b = new Vector(p.X - right.X, p.Y - right.Y);
        //    return Math.Acos((a.X * b.X + a.Y * b.Y) / Math.Sqrt(a.X*a.X+a.Y*a.Y)*Math.Sqrt(b.X*b.X+b.Y*b.Y));
        //}

        /// <summary>
        /// Reset temporary calibration data
        /// </summary>
        public void Reset()
        {
            foreach (List<Point> l in _calibratorData)
            {
                l.Clear();
            }
        }

        /// <summary>
        /// Add a point from calibration
        /// </summary>
        /// <param name="screenX">Beamer coordinate</param>
        /// <param name="screenY">Beamer coordinate</param>
        /// <param name="imgX">Webcam coordinate</param>
        /// <param name="imgY"></param>
        public void AddPoint(int screenX, int screenY, int imgX, int imgY)
        {
            if (_calibratorData[imgX,imgY] == null)
                _calibratorData[imgX, imgY] = new List<Point>();
            _calibratorData[imgX,imgY].Add(new Point{X = screenX, Y = screenY});
            AddRefPoints(screenX, screenY);
        }

        /// <summary>
        /// Add a point from calibration
        /// </summary>
        /// <param name="screen">Beamer coordinate</param>
        /// <param name="img">Webcam coordinate</param>
        public void AddPoint(Point screen, Point img)
        {
#if DEBUG
            using (var fs = new StreamWriter(new FileStream(@"C:\Temp\aforge\points.csv", FileMode.Append, FileAccess.Write)))
            {
                var s = screen.X + ";" + screen.Y + ";" + img.X + ";" + img.Y + ";";
                fs.WriteLine(s);
                fs.Flush();
            }
#endif
            if (_calibratorData[(int)img.X, (int)img.Y] == null)
            {
                _calibratorData[(int) img.X, (int) img.Y] = new List<Point>();
            }
            _calibratorData[(int) img.X, (int) img.Y].Add(new Point { X = screen.X, Y = screen.Y });
            AddRefPoints((int) screen.X, (int) screen.Y);
        }

        private void AddRefPoints(int screenX, int screenY)
        {
            if (_refPoints.ContainsKey(screenX))
            {
                _refPoints[screenX].Add(screenY);
            }
            else
            {
                _refPoints.Add(screenX, new SortedSet<int>());
                _refPoints[screenX].Add(screenY);
            }
        }

        /// <summary>
        /// Calibrate from collected data
        /// </summary>
        public void Calculate()
        {
            for (int i = 0; i < _mapData.GetLength(0); i++)
            {
                for (int j = 0; j < _mapData.GetLength(1); j++)
                {
                    var k = new Point();
                    k.X = _calibratorData[i, j].Sum(x => x.X)/_calibratorData[i, j].Count;
                    k.Y = _calibratorData[i, j].Sum(x => x.Y)/_calibratorData[i, j].Count;
                    _mapData[i, j] = k;
                }
            }
            _refPoints.Clear();
        }

        /// <summary>
        /// Calculate from corners
        /// </summary>
        public void PredictFromCorners()
        {
            // TODO index out of bound exception möglich
            var stor = _calibratorData.Clone();
            int xmax = (int) _calibratorData[(int) BottomRight.X, (int) BottomRight.Y].First().X;
            int ymax = (int) _calibratorData[(int) BottomRight.X, (int) BottomRight.Y].First().Y;
            for (int i = 0; i < _calibratorData.GetLength(0); i++)
            {
                for (int j = 0; j < _calibratorData.GetLength(1); j++)
                {
                    if (_calibratorData[i, j]!=null)
                    {
                        _calibratorData[i, j].Clear();
                    }
                    else
                    {
                        _calibratorData[i, j] = new List<Point>();
                    }
                }
            }
            for (int i = 0; i < xmax; i++)
            {
                for (int j = 0; j < ymax; j++)
                {
                    //find intersection
                    int y1 = (int)Math.Round(TopLeft.Y - (double)(TopLeft.Y - TopRight.Y) * ((double)i / (double)xmax));
                    int y2 = (int)Math.Round(BottomLeft.Y - (double)(BottomLeft.Y - BottomRight.Y) * ((double)i / (double)xmax));
                    int x1 = (int)Math.Round(TopLeft.X - (double)(TopLeft.X - TopRight.X) * ((double)i / (double)xmax));
                    int x2 = (int)Math.Round(BottomLeft.X - (double)(BottomLeft.X - BottomRight.X) * ((double)i / (double)xmax));
                    int y3 = (int)Math.Round(TopLeft.Y - (double)(TopLeft.Y - BottomLeft.Y) * ((double)j / (double)ymax));
                    int y4 = (int)Math.Round(TopRight.Y - (double)(TopRight.Y - BottomRight.Y) * ((double)j / (double)ymax));
                    int x3 = (int)Math.Round(TopLeft.X - (double)(TopLeft.X - BottomLeft.X) * ((double)j / (double)ymax));
                    int x4 = (int)Math.Round(TopRight.X - (double)(TopRight.X - BottomRight.X) * ((double)j / (double)ymax));
                    int A1 = y2 - y1;
                    int B1 = x1 - x2;
                    int C1 = A1*x1 + B1*y1;
                    int A2 = y4 - y3;
                    int B2 = x3 - x4;
                    int C2 = A2 * x3 + B2 * y3;
                    double det = A1*B2 - A2*B1;
                        int x = (int) Math.Round((B2*C1 - B1*C2)/det);
                        int y = (int) Math.Round((A1*C2 - A2*C1)/det);
                    _calibratorData[x, y].Add(new Point(i,j));
                }
            }
            Calculate();
            _calibratorData = (List<Point>[,]) stor;
        }

        /// <summary>
        /// Get the calculated screen coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Point GetPosition(int x, int y)
        {
            return _mapData[x, y];
        }

        public bool Contains(AForge.Point centerOfGravity)
        {
            return Contains(new Point(centerOfGravity.X, centerOfGravity.Y));
        }
    }
}
