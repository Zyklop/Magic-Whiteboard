using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge;

namespace HSR.PresWriter.PenTracking.Mappers
{
    public class LinearMapper:AbstractPointMapper
    {
        private SortedDictionary<int, SortedDictionary<int, List<System.Drawing.Point>>> _calibratorData;
        private System.Drawing.Point[,] _mapData;

        public LinearMapper(Grid grid) : base(grid)
        {
            Calculate();
        }

        public override Point FromPresentation(Point p)
        {
            var x = (int)Math.Round(p.X);
            var y = (int)Math.Round(p.Y);
            if (x == 0 && y == 0)
                return new Point(-1, -1);
            return new Point(_mapData[x, y].X, _mapData[x, y].Y);
        }

        public void Calculate()
        {
            int xmax = Grid.ScreenSize.Width;
            int ymax = Grid.ScreenSize.Height;
            _calibratorData = new SortedDictionary<int, SortedDictionary<int, List<System.Drawing.Point>>>();
            _mapData = new System.Drawing.Point[xmax, ymax];
            //copy calibrator data after backup
            for (int i = 0; i < xmax; i += 2)
            {
                //Debug.WriteLine(i);
                for (int j = 0; j < ymax; j += 2)
                {
                    //Debug.WriteLineIf(i > 1278, j);
                    //find intersection
                    var y1 = (int)Math.Round(Grid.TopLeft.Y - (Grid.TopLeft.Y - Grid.TopRight.Y) * (i / (double)xmax));
                    var y2 = (int)Math.Round(Grid.BottomLeft.Y - (Grid.BottomLeft.Y - Grid.BottomRight.Y) * (i / (double)xmax));
                    var x1 = (int)Math.Round(Grid.TopLeft.X - (Grid.TopLeft.X - Grid.TopRight.X) * (i / (double)xmax));
                    var x2 = (int)Math.Round(Grid.BottomLeft.X - (Grid.BottomLeft.X - Grid.BottomRight.X) * (i / (double)xmax));
                    var y3 = (int)Math.Round(Grid.TopLeft.Y - (Grid.TopLeft.Y - Grid.BottomLeft.Y) * (j / (double)ymax));
                    var y4 = (int)Math.Round(Grid.TopRight.Y - (Grid.TopRight.Y - Grid.BottomRight.Y) * (j / (double)ymax));
                    var x3 = (int)Math.Round(Grid.TopLeft.X - (Grid.TopLeft.X - Grid.BottomLeft.X) * (j / (double)ymax));
                    var x4 = (int)Math.Round(Grid.TopRight.X - (Grid.TopRight.X - Grid.BottomRight.X) * (j / (double)ymax));
                    var a1 = y2 - y1;
                    var b1 = x1 - x2;
                    var c1 = a1 * x1 + b1 * y1;
                    var a2 = y4 - y3;
                    var b2 = x3 - x4;
                    var c2 = a2 * x3 + b2 * y3;
                    double det = a1 * b2 - a2 * b1;
                    var x = (int)Math.Round((b2 * c1 - b1 * c2) / det);
                    var y = (int)Math.Round((a1 * c2 - a2 * c1) / det);
                    try
                    {
                        AddCalibratorPoint(x, y, new System.Drawing.Point(i, j));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Tried to add " + x + ", " + y + " and " + e.Message + " happend");
                    }
                }
            }
            SetMapData();
        }

        private void AddCalibratorPoint(int imgX, int imgY, System.Drawing.Point screen)
        {
            if (!_calibratorData.ContainsKey(imgX))
                _calibratorData.Add(imgX, new SortedDictionary<int, List<System.Drawing.Point>>());
            if (!_calibratorData[imgX].ContainsKey(imgY))
                _calibratorData[imgX].Add(imgY, new List<System.Drawing.Point> { screen });
            else
                _calibratorData[imgX][imgY].Add(screen);
        }

        private void SetMapData()
        {
            for (int i = 0; i < _mapData.GetLength(0); i++)
            {
                for (int j = 0; j < _mapData.GetLength(1); j++)
                {
                    if (_calibratorData.ContainsKey(i) && _calibratorData[i].ContainsKey(j) && _calibratorData[i][j].Count > 0)
                    {
                        try
                        {
                            //var k = new Point();
                            //k.X = _calibratorData[i][j].Sum(x => x.X)/_calibratorData[i][j].Count;
                            //k.Y = _calibratorData[i][j].Sum(x => x.Y)/_calibratorData[i][j].Count;
                            _mapData[i, j] = Average(_calibratorData[i][j]);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message + " happend during grid calculation at " + i + ", " + j);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Average position
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private System.Drawing.Point Average(List<System.Drawing.Point> points)
        {
            if (points.Count == 0)
                throw new ArgumentException("List is empty");
            if (points.Count == 1)
                return points.First();
            var res = new System.Drawing.Point(points.Sum(x => x.X), points.Sum(x => x.Y));
            res.X = (int)Math.Round(res.X * 1.0 / points.Count);
            res.Y = (int)Math.Round(res.Y * 1.0 / points.Count);
            return res;
        }

        /// <summary>
        /// Weighted Average
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private System.Drawing.Point Average(Dictionary<System.Drawing.Point, double> points)
        {
            if (points.Count == 0)
                throw new ArgumentException("List is empty");
            if (points.Count == 1)
                return points.First().Key;
            var rx = points.Sum(x => x.Value * x.Key.X);
            var ry = points.Sum(x => x.Value * x.Key.Y);
            var sWeights = points.Sum(x => x.Value);
            return new System.Drawing.Point((int)Math.Round(rx / sWeights), (int)Math.Round(ry / sWeights));
        }
    }
}
