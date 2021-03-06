﻿using HSR.PresWriter;
using HSR.PresWriter.Containers;
using HSR.PresWriter.IO;
using HSR.PresWriter.IO.Cameras;
using HSR.PresWriter.IO.Events;
using HSR.PresWriter.PenTracking;
using HSR.PresWriter.PenTracking.Events;
using HSR.PresWriter.PenTracking.Strategies;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WFVisuslizer;
using InputEmulation;
using Point = System.Drawing.Point;

namespace WinFormsGuiTester
{
    public partial class LiveTestForm : Form
    {
        private AForgeCamera _camera;
        private DataParser _parser;
        private FixedSizedQueue<PointFrame> penDrawingBuffer;
        private Form screenForm;
        private AdvancedInputEmulator _inputEmulator;
        private bool InputEnabled = false;

        public LiveTestForm()
        {
            this.penDrawingBuffer = new FixedSizedQueue<PointFrame>(200);
            InitializeComponent();

            // Initialize Camera
            _camera = new AForgeCamera();
            //_camera = new FilesystemCamera(new DirectoryInfo(@"C:\temp\aforge\inph"));
            _camera.FrameReady += _camera_FrameReady;
            /*
            _camera.FrameReady += delegate(object o, FrameReadyEventArgs e) {
                String path = @"C:\temp\live\cap-" + DateTime.Now.DayOfYear;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                e.Frame.Bitmap.Save(Path.Combine(path, "cap-" + CurrentMillis.Millis + ".png"));
            };
            */

            // Initialize Calibration and Pen Parsing Mechanism
            //_parser = new DataParser(
            //    new SimpleAForgeCalibrator(_camera, VisualizerControl.GetVisualizer()), 
            //    new AForgePenTracker(new RedLaserStrategy(), _camera));
            _parser = new DataParser(_camera, VisualizerControl.GetVisualizer());
            //_parser = new QuadrilateralDataParser(_camera, VisualizerControl.GetVisualizer());
            _parser.PenPositionChanged += parser_PenPositionChanged;

            // Form for visual feedback of tracking process
            //screenForm = new ScreenForm();
            _inputEmulator = new AdvancedInputEmulator(VisualizerControl.GetVisualizer().Width, VisualizerControl.GetVisualizer().Height);

            if (!File.Exists(@"C:\temp\foundpoints.csv"))
            {
                File.Create(@"C:\temp\foundpoints.csv");
            }
        }

        private void _camera_FrameReady(object sender, FrameReadyEventArgs e)
        {


            Bitmap redaction = (Bitmap)e.Frame.Bitmap.Clone();

            // draw points in buffer to image
            using (Graphics g = Graphics.FromImage(redaction))
            {
                if (_parser.CalibratorGrid != null)
                {
                    using (SolidBrush brush = new SolidBrush(Color.Black))
                    {
                        Point bottomLeft = _parser.CalibratorGrid.BottomLeft;
                        Point topLeft = _parser.CalibratorGrid.TopLeft;
                        Point bottomRight = _parser.CalibratorGrid.BottomRight;
                        Point topRight = _parser.CalibratorGrid.TopRight;
                        g.DrawLine(Pens.Red, bottomLeft, bottomRight);
                        g.DrawLine(Pens.Red, bottomLeft, topLeft);
                        g.DrawLine(Pens.Red, topRight, bottomRight);
                        g.DrawLine(Pens.Red, topRight, topLeft);
                    }
                }


                // TODO: Object is currently in use elsewhere..
                this.cameraPictureBox.Image = redaction;
            }

        }

        //[DllImport("User32.dll")]
        //public static extern IntPtr GetDC(IntPtr hwnd);

        //[DllImport("User32.dll")]
        //public static extern void ReleaseDC(IntPtr dc);

        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    IntPtr desktopDC = GetDC(IntPtr.Zero);

        //    using (Graphics g = Graphics.FromHdc(desktopDC))
        //    {
        //        using (SolidBrush brush = new SolidBrush(Color.Green))
        //        {
        //            Point previousPoint = Point.Empty;
        //            foreach (PointFrame f in penDrawingBuffer)
        //            {
        //                g.DrawEllipse(Pens.Green, f.Point.X - 3, f.Point.Y - 3, 3, 3);
        //                if (!previousPoint.IsEmpty && PointTools.CalculateDistance(previousPoint, f.Point) < 50)
        //                {
        //                    g.DrawLine(Pens.Red, previousPoint, f.Point);
        //                }
        //                previousPoint = f.Point;
        //            }
        //        }
        //    }
        //    //ReleaseDC(desktopDC);
        //}

        private void parser_PenPositionChanged(object sender, PenPositionEventArgs e)
        {
            if(e.Frame == null)
            {
                _inputEmulator.NoData();
                return;
            }

            this.foundPointLabel.Text = "Found Point: " + e.Frame.Point.X + ", " + e.Frame.Point.Y;

            // Write Values to CSV
            using (var fs = new StreamWriter(new FileStream(@"C:\temp\foundpoints.csv", FileMode.Append, FileAccess.Write)))
            {
                fs.WriteLine(e.Frame.Timestamp+";"+e.Frame.Point.X+";"+e.Frame.Point.Y);
                fs.Flush();
            }

            if (InputEnabled)
            {
                _inputEmulator.NewPoint(e.Frame.Point);
            }

            //Bitmap redaction = (Bitmap)this.cameraPictureBox.Image.Clone();
            //if (e.Frame != null)
            //{
            //    penDrawingBuffer.Enqueue(e.Frame);
            //}

            //// draw points in buffer to image
            //using (Graphics g = screenForm.CreateGraphics())
            //{
            //    g.Clear(Color.Black);
            //    using (SolidBrush brush = new SolidBrush(Color.Green))
            //    {
            //        Point previousPoint = Point.Empty;
            //        foreach (PointFrame f in penDrawingBuffer)
            //        {
            //            g.DrawEllipse(Pens.Green, f.Point.X - 3, f.Point.Y - 3, 3, 3);
            //            if (!previousPoint.IsEmpty && PointTools.CalculateDistance(previousPoint, f.Point) < 100)
            //            {
            //                g.DrawLine(Pens.Red, previousPoint, f.Point);
            //            }
            //            previousPoint = f.Point;
            //        }
            //    }
            //    this.cameraPictureBox.Image = redaction;
            //}


            //var vc = VisualizerControl.GetVisualizer();
            //Task.Factory.StartNew(() =>
            //{
            //    vc.Clear();
            //    vc.Show();
            //    vc.Transparent = true;
            //    //vc.MarkPoint(e.Frame.Point.X, e.Frame.Point.Y);
            //    vc.AddRect(e.Frame.Point.X, e.Frame.Point.Y, 25, 25, Color.Green);
            //    vc.Close();
            //    //    Dispatcher.Run();
            //});
            //thread2.SetApartmentState(ApartmentState.STA);
            //thread2.Start();

            //VisualizerControl v = VisualizerControl.GetVisualizer();
            //v.Transparent = true;
            //v.Clear();
            //v.AddRect(e.Frame.Point.X, e.Frame.Point.Y, 2, 2, Color.Green);
            //v.Show();
            //v.Draw();

            //// draw points in buffer to image
            //using (Graphics g = Graphics.FromImage(redaction))
            //{
            //    using (SolidBrush brush = new SolidBrush(Color.Black))
            //    {
            //        Point previousPoint = Point.Empty;
            //        foreach (PointFrame f in penDrawingBuffer)
            //        {
            //            g.DrawEllipse(Pens.Green, f.Point.X - 3, f.Point.Y - 3, 3, 3);
            //            if (!previousPoint.IsEmpty && PointTools.CalculateDistance(previousPoint, f.Point) < 50)
            //            {
            //                g.DrawLine(Pens.Red, previousPoint, f.Point);
            //            }
            //            previousPoint = f.Point;
            //        }
            //    }
            //    this.cameraPictureBox.Image = redaction;
            //}

        }

        private void toggleCameraButton_Click(object sender, EventArgs e)
        {
            if (!_camera.IsRunning)
            {
                _camera.Start();
                this.toggleCameraButton.Text = "Stop Camera";
            }
            else
            {
                _camera.Stop();
                this.toggleCameraButton.Text = "Start Camera";
            }
        }

        private void toggleParserButton_Click(object sender, EventArgs e)
        {
            if (!_parser.IsRunning)
            {
                _parser.Start();
                this.toggleParserButton.Text = "Stop Parser";
            }
            else
            {
                _parser.Stop();
                this.toggleParserButton.Text = "Start Parser";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _camera.ShowConfigurationDialog();
        }

        private void overlayButton_Click(object sender, EventArgs e)
        {
            InputEnabled = !InputEnabled;
        }
    }
}
