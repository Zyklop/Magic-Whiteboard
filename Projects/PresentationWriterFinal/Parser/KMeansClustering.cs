using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AForge;

namespace HSR.PresWriter.PenTracking
{
    internal static class KMeansCluster
    {
        /// <summary>
        /// Finds KMeans Clusters
        /// </summary>
        /// <param name="points">Collection of the Points</param>
        /// <param name="clusterCenters">Centers of the Clusters (Prediced)</param>
        /// <param name="maxIterations">Number of iterations to calculate</param>
        /// <returns>The index of the cluster to the corresponding point</returns>
        public static List<Cluster> KMeansClustering(List<Point> points, List<Point> clusterCenters,
                                                     int maxIterations = 1)
        {
            if (clusterCenters.Count == 0)
                throw new ArgumentException("No clusters defined");
            var changed = true;
            var clusters = new List<Cluster>();
            var allpoints = new List<Point>(points.Distinct());
            foreach (var point in clusterCenters)
            {
                clusters.Add(new Cluster(point));
                //remove possible duplicates
                allpoints.Remove(point);
            }
            // fill all points in a random cluster, just for the initialisation
            clusters[0].Points.AddRange(allpoints);
            // they get distributed correctly in the loop
            for (int ct = 0; changed && ct < maxIterations; ct++)
            {
                changed = Assign(clusters);
                UpdateCentroids(clusters);
            }
            return clusters;
        }

        private static void UpdateCentroids(IEnumerable<Cluster> clusters)
        {
            foreach (var cluster in clusters)
            {
                cluster.UpdateCentroid();
            }
        }

        private static bool Assign(List<Cluster> clusters)
        {
            bool changed = false;
            Cluster minCluster = null;
            foreach (var cluster in clusters)
            {
                var toRemove = new List<Point>();
                foreach (var p in cluster.Points)
                {
                    var minDist = Double.MaxValue;
                    foreach (var innerCluster in clusters)
                    {
                        var currDist = p.DistanceTo(innerCluster.Centroid);
                        if (currDist < minDist)
                        {
                            minDist = currDist;
                            minCluster = innerCluster;
                        }
                    }
                    if (minCluster != cluster)
                    {
                        Debug.Assert(minCluster != null, "somthing went terribly wrong, while changing a cluster");
                        minCluster.Points.Add(p);
                        changed = true;
                        // remove after to not modify the collection
                        toRemove.Add(p);
                    }
                }
                cluster.Points.RemoveAll(toRemove.Contains);
            }
            return changed;
        }
    }

    internal class Cluster
    {
        public Cluster(Point point) : this()
        {
            Centroid = point;
            Points.Add(point);
        }

        protected Cluster()
        {
            Points = new List<Point>();
        }

        public List<Point> Points { get; set; }

        public Point Centroid { get; set; }

        public Point Mean
        {
            get
            {
                if (Points.Count == 0)
                    return Centroid;
                var s = new Point(Points.Average(x => x.X), Points.Average(x => x.Y));
                return s;
            }
        }

        internal void UpdateCentroid()
        {
            var centroid = new Point();
            double minDist = double.MaxValue;
            foreach (var point in Points)
            {
                double currDist = point.DistanceTo(Mean);
                if (currDist < minDist)
                {
                    minDist = currDist;
                    centroid = point;
                }
            }
            Centroid = centroid;
        }
    }
}
