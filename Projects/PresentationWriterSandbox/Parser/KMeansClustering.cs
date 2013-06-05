using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static List<Cluster> KMeansClustering(List<AForge.Point> points, List<AForge.Point> clusterCenters,
                                                     int maxIterations = 1)
        {
            if (clusterCenters.Count == 0)
                throw new ArgumentException("No clusters defined");
            var changed = true;
            var clusters = new List<Cluster>();
            var allpoints = new List<AForge.Point>(points.Distinct());
            foreach (var point in clusterCenters)
            {
                clusters.Add(new Cluster(point));
                //remove possible duplicates
                allpoints.Remove(point);
            }
            // fill all points in a random cluster, just for the initialisation
            clusters[0].Points.AddRange(allpoints);
            // they get distributed correctly in the loop
            //changed = Assign(clusters);
            //UpdateCentroids(clusters);
            for (int ct = 0; changed && ct < maxIterations; ct++)
            {
                changed = Assign(clusters);
                UpdateCentroids(clusters);
            }
            return clusters;
        }

        private static void UpdateCentroids(List<Cluster> clusters)
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
                var toRemove = new List<AForge.Point>();
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

        /// <summary>
        /// Assign Points to their nearest center
        /// </summary>
        /// <param name="points"></param>
        /// <param name="clustering"></param>
        /// <param name="centroids"></param>
        /// <returns>True if somethich changed</returns>
        private static bool Assign(List<AForge.Point> points, List<int> clustering, List<AForge.Point> centroids)
        {
            bool changed = false;
            var distances = new List<double>(Enumerable.Repeat(0.0, centroids.Count));
            for (int i = 0; i < points.Count; i++)
            {
                foreach (var c in centroids)
                    distances[i] = (points[i].DistanceTo(c));
                int newCluster = distances.IndexOf(distances.Min());
                if (newCluster != clustering[i])
                {
                    changed = true;
                    clustering[i] = newCluster;
                }
            }
            return changed;
        }

        /// <summary>
        /// Seting new Points as Centroids
        /// </summary>
        /// <param name="points"></param>
        /// <param name="clustering"></param>
        /// <param name="centroids"></param>
        /// <param name="means"></param>
        private static void UpdateCentroids(List<AForge.Point> points, List<int> clustering,
                                            List<AForge.Point> centroids, List<AForge.Point> means)
        {
            for (int cluster = 0; cluster < centroids.Count; cluster++)
            {
                var centroid = new AForge.Point();
                var minDist = double.MaxValue;
                for (int i = 0; i < points.Count; i++)
                {
                    int c = clustering[i];
                    if (c != cluster)
                        continue;
                    double currDist = points[i].DistanceTo(means[cluster]);
                    if (currDist < minDist)
                    {
                        minDist = currDist;
                        centroid = points[i];
                    }
                }
                centroids[cluster] = centroid;
            }
        }

        /// <summary>
        /// Updating the mean center of the centroid
        /// </summary>
        /// <param name="points"></param>
        /// <param name="clustering"></param>
        /// <param name="means"></param>
        private static void UpdateMeans(List<AForge.Point> points, List<int> clustering, List<AForge.Point> means)
        {
            means = new List<AForge.Point>(Enumerable.Repeat(new AForge.Point(), means.Count));
            var clusterCounts = new List<int>(Enumerable.Repeat(0, means.Count));
            for (int i = 0; i < points.Count; i++)
            {
                var cluster = clustering[i];
                ++clusterCounts[cluster];
                means[cluster] += points[i];
            }
            for (int k = 0; k < means.Count; ++k)
                if (clusterCounts[k] != 0)
                    means[k] /= clusterCounts[k];
        }


        ///// <summary>
        ///// Finds KMeans Clusters
        ///// </summary>
        ///// <param name="points">Collection of the Points</param>
        ///// <param name="clusterCenters">Centers of the Clusters (Prediced)</param>
        ///// <param name="maxIterations">Number of iterations to calculate</param>
        ///// <returns>The index of the cluster to the corresponding point</returns>
        //public static List<Cluster> KMeansClustering(List<AForge.Point> points, List<AForge.Point> clusterCenters,
        //                                             int maxIterations = 1)
        //{
        //    if (clusterCenters.Count == 0)
        //        throw new ArgumentException("No clusters defined");
        //    var changed = true;
        //    var clusters = new List<Cluster>();
        //    var allpoints = new List<AForge.Point>(points);
        //    foreach (var point in clusterCenters)
        //    {
        //        clusters.Add(new Cluster(point));
        //        //remove possible duplicates
        //        allpoints.Remove(point);
        //    }
        //    // fill all points in a random cluster, just for the initialisation
        //    clusters[0].Points.AddRange(allpoints);
        //    // they get distributed correctly in the loop
        //    //changed = Assign(clusters);
        //    //UpdateCentroids(clusters);
        //    for (int ct = 0; changed && ct < maxIterations; ct++)
        //    {
        //        changed = Assign(clusters);
        //        UpdateCentroids(clusters);
        //    }
        //    return clusters;
        //}

        //private static void UpdateCentroids(List<Cluster> clusters)
        //{
        //    foreach (var cluster in clusters)
        //    {
        //        cluster.UpdateCentroid();
        //    }
        //}

        //private static bool Assign(List<Cluster> clusters)
        //{
        //    bool changed = false;
        //    Cluster minCluster = null;
        //    foreach (var cluster in clusters)
        //    {
        //        var toRemove = new List<AForge.Point>();
        //        foreach (var p in cluster.Points)
        //        {
        //            var minDist = Double.MaxValue;
        //            foreach (var innerCluster in clusters)
        //            {
        //                var currDist = p.DistanceTo(innerCluster.Centroid);
        //                if (currDist < minDist)
        //                {
        //                    minDist = currDist;
        //                    minCluster = innerCluster;
        //                }
        //            }
        //            if (minCluster != cluster)
        //            {
        //                Debug.Assert(minCluster != null, "somthing went terribly wrong, while changing a cluster");
        //                minCluster.Points.Add(p);
        //                changed = true;
        //                // remove after to not modify the collection
        //                toRemove.Add(p);
        //            }
        //        }
        //        cluster.Points.RemoveAll(toRemove.Contains);
        //    }
        //    return changed;
        //}

        ///// <summary>
        ///// Assign Points to their nearest center
        ///// </summary>
        ///// <param name="points"></param>
        ///// <param name="clustering"></param>
        ///// <param name="centroids"></param>
        ///// <returns>True if somethich changed</returns>
        //private static bool Assign(List<AForge.Point> points, List<int> clustering, List<AForge.Point> centroids)
        //{
        //    bool changed = false;
        //    var distances = new List<double>(Enumerable.Repeat(0.0, centroids.Count));
        //    for (int i = 0; i < points.Count; i++)
        //    {
        //        foreach (var c in centroids)
        //            distances[i] = (points[i].DistanceTo(c));
        //        int newCluster = distances.IndexOf(distances.Min());
        //        if (newCluster != clustering[i])
        //        {
        //            changed = true;
        //            clustering[i] = newCluster;
        //        }
        //    }
        //    return changed;
        //}

        ///// <summary>
        ///// Seting new Points as Centroids
        ///// </summary>
        ///// <param name="points"></param>
        ///// <param name="clustering"></param>
        ///// <param name="centroids"></param>
        ///// <param name="means"></param>
        //private static void UpdateCentroids(List<AForge.Point> points, List<int> clustering,
        //                                    List<AForge.Point> centroids, List<AForge.Point> means)
        //{
        //    for (int cluster = 0; cluster < centroids.Count; cluster++)
        //    {
        //        var centroid = new AForge.Point();
        //        var minDist = double.MaxValue;
        //        for (int i = 0; i < points.Count; i++)
        //        {
        //            int c = clustering[i];
        //            if (c != cluster)
        //                continue;
        //            double currDist = points[i].DistanceTo(means[cluster]);
        //            if (currDist < minDist)
        //            {
        //                minDist = currDist;
        //                centroid = points[i];
        //            }
        //        }
        //        centroids[cluster] = centroid;
        //    }
        //}

        ///// <summary>
        ///// Updating the mean center of the centroid
        ///// </summary>
        ///// <param name="points"></param>
        ///// <param name="clustering"></param>
        ///// <param name="means"></param>
        //private static void UpdateMeans<T>(List<T> points, List<int> clustering, List<T> means) where T : new()
        //{
        //    means = new List<T>(Enumerable.Repeat(new T(), means.Count));
        //    var clusterCounts = new List<int>(Enumerable.Repeat(0, means.Count));
        //    for (int i = 0; i < points.Count; i++)
        //    {
        //        var cluster = clustering[i];
        //        ++clusterCounts[cluster];
        //        (dynamic) means[cluster] += (dynamic) points[i];
        //    }
        //    for (int k = 0; k < means.Count; ++k)
        //        if (clusterCounts[k] != 0)
        //            (dynamic) means[k] /= clusterCounts[k];
        //}
    }

    internal class Cluster
    {
        public Cluster(AForge.Point point) : this()
        {
            Centroid = point;
            Points.Add(point);
        }

        protected Cluster()
        {
            Points = new List<AForge.Point>();
        }

        public List<AForge.Point> Points { get; set; }

        public AForge.Point Centroid { get; set; }

        public AForge.Point Mean
        {
            get
            {
                if (Points.Count == 0)
                    return Centroid;
                var s = new AForge.Point(Points.Average(x => x.X), Points.Average(x => x.Y));
                return s;
            }
        }

        internal void UpdateCentroid()
        {
            var centroid = new AForge.Point();
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

    //internal class Cluster<T>
    //{
    //    public Cluster(T centroid)
    //        : this()
    //    {
    //        Centroid = centroid;
    //        Points.Add(centroid);
    //    }

    //    protected Cluster()
    //    {
    //        Points = new List<T>();
    //    }

    //    public List<T> Points { get; set; }

    //    public T Centroid { get; set; }

    //    public AForge.Point Mean
    //    {
    //        get
    //        {
    //            if (Points.Count == 0)
    //                return Centroid;
    //            var s = new AForge.Point(Points.Average(x => x.X), Points.Average(x => x.Y));
    //            return s;
    //        }
    //    }

    //    internal void UpdateCentroid()
    //    {
    //        var centroid = new AForge.Point();
    //        double minDist = double.MaxValue;
    //        foreach (var point in Points)
    //        {
    //            double currDist = point.DistanceTo(Mean);
    //            if (currDist < minDist)
    //            {
    //                minDist = currDist;
    //                centroid = point;
    //            }
    //        }
    //        Centroid = centroid;
    //    }
    //}

}
