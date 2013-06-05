using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
