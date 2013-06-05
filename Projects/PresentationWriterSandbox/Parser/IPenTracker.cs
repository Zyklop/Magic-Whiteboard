using HSR.PresWriter.Containers;
using HSR.PresWriter.IO.Events;
using HSR.PresWriter.PenTracking.Events;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace HSR.PresWriter.PenTracking
{
    /// <summary>
    /// Everthing finding points on a picture implements this interface.
    /// It is not defined, where the picture input (if one) for the tracking 
    /// process has to come from.
    /// - Tracking starts as the Start-Method is called and stops as the Stop-Method is called.
    /// - Found Points are to be made public per PenFound-Event.
    /// </summary>
    public interface IPenTracker
    {
        /// <summary>
        /// References the previously (last) found frame
        /// </summary>
        PointFrame GetLastFound();

        /// <summary>
        /// Return an interpolated NOT REALLY FOUND point at a given time.
        /// </summary>
        /// <param name="timestamp">Timestamp for the interpolation</param>
        Point GetPenPoint(long timestamp);

        /// <summary>
        /// Fired when a pen was found definitively. Quality before quantity!
        /// </summary>
        event EventHandler<PenFoundEventArgs> PenFound;

        /// <summary>
        /// Fired when no pen or was found, or when we are not sure if we found one.
        /// </summary>
        event EventHandler<EventArgs> NoPenFound;

        /// <summary>
        /// Start searching for pen points.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop searching for pen points.
        /// </summary>
        void Stop();
    }
}
