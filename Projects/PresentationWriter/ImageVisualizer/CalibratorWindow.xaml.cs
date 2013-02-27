using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageVisualizer
{
    /// <summary>
    /// Interaction logic for CalibratorWindow.xaml
    /// </summary>
    public partial class CalibratorWindow : Window
    {

        public CalibratorWindow()
        {
            InitializeComponent();
        }

        public void AddRect(int x, int y, int width, int height, Color c)
        {
            var r = new Rectangle();
            r.Width = width;
            r.Height = height;
            r.Margin = new Thickness(x,y,0,0);
            r.Fill = new SolidColorBrush(c);
            MainGrid.Children.Add(r);
        }

        public void ClearRects()
        {
            MainGrid.Children.Clear();
        }

        public bool Transparent { get { return this.Background.Opacity == 0.0; } set { Background.Opacity = value ? 0.0 : 1.0; } }
    }
}
