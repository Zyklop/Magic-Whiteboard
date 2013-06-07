using System.Windows;

namespace HSR.PresWriter.ImageVisualizer
{
    /// <summary>
    /// Interaction logic for VisualizerApp.xaml
    /// </summary>
    public partial class VisualizerApp : Application
    {
        public VisualizerApp()
        {
            CalibratorWindow = new CalibratorWindow {Topmost = true};
            CalibratorWindow.Show();
            CalibratorWindow.Hide();
        }

        public CalibratorWindow CalibratorWindow { get; set; }
    }
}
