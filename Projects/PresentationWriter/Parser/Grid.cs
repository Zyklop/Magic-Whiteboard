using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresentationWriter.Parser
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

        public Grid(int width, int height)
        {
            _mapData = new Point[width,height];
            _calibratorData = new List<Point>[width,height];
            _refPoints = new SortedDictionary<int, SortedSet<int>>();
        }

        public int Width { get
        {
            return (int) (TopRight.X - TopLeft.X > BottomRight.X - BottomLeft.X
                              ? TopRight.X - TopLeft.X
                              : BottomRight.X - BottomLeft.X);
        } }

        public int Height
        {
            get
            {
                return (int) (BottomLeft.Y - TopLeft.Y > BottomRight.Y - TopRight.Y
                                  ? BottomLeft.Y - TopLeft.Y
                                  : BottomRight.Y - TopRight.Y);
            }
        }

        public bool DataNeeded { get { return _needed; } }

        public Rect Needed()
        {
            return new Rect();
        }

        public Point TopLeft
        {
            get { return _topLeft; }
            set { 
                _topLeft = value;
            }
        }

        public Point TopRight
        {
            get { return _topRight; }
            set
            {
                _topRight = value;
            }
        }

        public Point BottomLeft
        {
            get { return _bottomLeft; }
            set
            {
                _bottomLeft = value;
            }
        }

        public Point BottomRight
        {
            get { return _bottomRight; }
            set
            {
                _bottomRight = value;
            }
        }

        public void Reset()
        {
            foreach (List<Point> l in _calibratorData)
            {
                l.Clear();
            }
        }

        public void AddPoint(int screenX, int screenY, int imgX, int imgY)
        {
            _calibratorData[imgX,imgY].Add(new Point{X = screenX, Y = screenY});
            AddRefPoints(screenX, screenY);
        }

        public void AddPoint(Point screen, Point img)
        {
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

        public void PredictFromCorners()
        {
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

        public Point GetPosition(int x, int y)
        {

            return _mapData[x, y];
        }
    }
}
