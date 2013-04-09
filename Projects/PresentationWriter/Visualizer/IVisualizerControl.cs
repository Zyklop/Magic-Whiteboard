using System.Drawing;

namespace Visualizer
{
    public interface IVisualizerControl
    {
        /// <summary>
        /// Set a transparent background
        /// </summary>
        bool Transparent { get; set; }

        /// <summary>
        /// Width of the shown window
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Height of the window when shown
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Show a rectangle
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="fromRgb"></param>
        void AddRect(Point topLeft, Point bottomRight, Color fromRgb);

        /// <summary>
        /// Show a rectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="c"></param>
        //void AddRect(Rect rect, Color c);

        /// <summary>
        /// Remove all the rectangles
        /// </summary>
        void Clear();

        /// <summary>
        /// Close the window when shown
        /// </summary>
        void Close();

        /// <summary>
        /// Showing the window if not already open
        /// </summary>
        void Show();

        /// <summary>
        /// Show a rectangle
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        void AddRect(int topLeft, int bottomRight, int width, int height, Color color);
    }
}
