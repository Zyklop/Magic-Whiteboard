﻿using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HSR.PresWriter.ImageVisualizer
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
            if (Dispatcher.Thread.GetApartmentState() != ApartmentState.STA)
            {
                //started from a non-sta context
                // very slow, when called from a sta context
                var t = new Thread(() =>
                    {
                        if (Dispatcher.CheckAccess())
                            MainGrid.Children.Add(new Rectangle
                                {
                                    Width = width,
                                    Height = height,
                                    Margin = new Thickness(x, y, 0, 0),
                                    Fill = new SolidColorBrush(c)
                                });
                        else
                            Dispatcher.Invoke(() =>
                                {
                                    MainGrid.Children.Add(new Rectangle
                                        {
                                            VerticalAlignment = VerticalAlignment.Top,
                                            HorizontalAlignment = HorizontalAlignment.Left,
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
            else
            {
                // Thread already has the sta-Attribute, this is much faster
                if (Dispatcher.CheckAccess())
                    MainGrid.Children.Add(new Rectangle
                    {
                        Width = width,
                        Height = height,
                        Margin = new Thickness(x, y, 0, 0),
                        Fill = new SolidColorBrush(c)
                    });
                else
                    Dispatcher.Invoke(() =>
                    {
                        MainGrid.Children.Add(new Rectangle
                        {
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Width = width,
                            Height = height,
                            Margin = new Thickness(x, y, 0, 0),
                            Fill = new SolidColorBrush(c)
                        });
                    });
            }
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
