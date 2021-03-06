﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace ImageVisualizer
{
    /// <summary>
    /// Interaction logic for CalibratorWindow.xaml
    /// </summary>
    partial class CalibratorWindow : Window
    {

        public CalibratorWindow()
        {
            InitializeComponent();
            Background = new SolidColorBrush(Color.FromRgb(0,0,0));
            AllowsTransparency = true;
        }

        public void AddRect(int x, int y, int width, int height, Color c)
        {
            var t = new Thread(() =>
                {
                    var r = new Rectangle
                        {
                            Width = width,
                            Height = height,
                            Margin = new Thickness(x, y, 0, 0),
                            Fill = new SolidColorBrush(c)
                        };
                    if (Dispatcher.CheckAccess())
                        MainGrid.Children.Add(r);
                    else
                        Dispatcher.Invoke(() =>
                            {
                                MainGrid.Children.Add(new Rectangle
                                    {
                                        Width = width,
                                        Height = height,
                                        Margin = new Thickness(x, y, 0, 0),
                                        Fill = new SolidColorBrush(c)
                                    });
                            });
                });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        public void Clear()
        {
            if (MainGrid.Dispatcher.CheckAccess())
                MainGrid.Children.Clear();
            else
                MainGrid.Dispatcher.Invoke(() => MainGrid.Children.Clear());
        }

        public bool Transparent { get { return Background.Opacity == 0.0; } set
        {
            if (Dispatcher.CheckAccess())
                Background = value ? new SolidColorBrush(Color.FromArgb(0, 0, 0, 0))
                                                         : new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            else
                Dispatcher.Invoke(() => Background = value
                                                         ? new SolidColorBrush(Color.FromArgb(0, 0, 0, 0))
                                                         : new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)));
        } }
    }
}
