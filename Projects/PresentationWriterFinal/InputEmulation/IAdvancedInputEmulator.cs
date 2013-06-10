using System;

namespace HSR.PresWriter.InputEmulation
{
    public interface IAdvancedInputEmulator
    {
        int RightClickTimeOut { get; set; }
        int ReleaseTimeout { get; set; }
        int KeyboardReleaseTimeout { get; set; }
        int Radius { get; set; }
        int BorderWidth { get; set; }
        int AverageCount { get; set; }

        /// <summary>
        /// No recent contact has been detected
        /// </summary>
        void NoData();

        /// <summary>
        /// A new contact has been detected
        /// </summary>
        /// <param name="p"></param>
        void NewPoint(System.Drawing.Point p);

        /// <summary>
        /// Menu has been toggled
        /// </summary>
        event EventHandler ShowMenu;

    }
}