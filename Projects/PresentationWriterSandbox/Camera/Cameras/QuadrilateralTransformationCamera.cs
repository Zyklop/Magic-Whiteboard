using System;
using System.Collections.Generic;
using System.Drawing;
using AForge;
using AForge.Imaging.Filters;
using HSR.PresWriter.Containers;
using HSR.PresWriter.IO;
using HSR.PresWriter.IO.Events;
using Point = System.Drawing.Point;

namespace HSR.PresWriter.DataSources.Cameras
{
    public class QuadrilateralTransformationCamera : ICamera
    {
        public event EventHandler<FrameReadyEventArgs> FrameReady;
        private List<IntPoint> _corners;
        private VideoFrame lastFrame;
        private SimpleQuadrilateralTransformation _filter;
        private int _height;
        private int _width;

        public QuadrilateralTransformationCamera(Point tl, Point tr, Point bl, Point br, double screenRatio)
        {
            _corners = new List<IntPoint>();
            _corners.Add(new IntPoint(tl.X, tl.Y));
            _corners.Add(new IntPoint(tr.X,tr.Y));
            _corners.Add(new IntPoint(br.X,br.Y));
            _corners.Add(new IntPoint(bl.X, bl.Y));
            Width = 640;
            Height = (int) Math.Round(Width/screenRatio);
            IsRunning = false;
            _filter =
                new SimpleQuadrilateralTransformation(_corners , Width, Height );
        }

        public int Height
        {
            get { return _height; }
            set 
            {
                _height = value;
                _filter =
                    new SimpleQuadrilateralTransformation(_corners, Width, Height);
            }
        }

        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                _filter =
                    new SimpleQuadrilateralTransformation(_corners, Width, Height);
            }
        }

        public void Dispose()
        {
        }

        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void TransformImage(VideoFrame vf)
        {
            lastFrame = new VideoFrame(vf.Number, _filter.Apply(vf.Bitmap), vf.Timestamp);
            FrameReady(this, new FrameReadyEventArgs(lastFrame));
        }

        public bool IsRunning { get; private set; }

        public VideoFrame GetLastFrame()
        {
            return lastFrame ?? new VideoFrame(0, new Bitmap(Width, Height));
        }
    }
}
