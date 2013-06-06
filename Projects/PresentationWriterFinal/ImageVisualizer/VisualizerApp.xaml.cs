using System.Windows;

namespace ImageVisualizer
{
    /// <summary>
    /// Interaction logic for VisualizerApp.xaml
    /// </summary>
    public partial class VisualizerApp : Application
    {
        public VisualizerApp()
        {
            CalibratorWindow = new CalibratorWindow();
            CalibratorWindow.Show();
            CalibratorWindow.Hide();
        }

        public CalibratorWindow CalibratorWindow { get; set; }
    }
}
