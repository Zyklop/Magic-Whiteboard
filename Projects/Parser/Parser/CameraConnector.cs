using System;
using Parser.Events;

namespace Parser
{
    internal class CameraConnector
    {
        public CameraConnector(Camera camera)
        {
        }

        public event EventHandler<NewImageEventArgs> NewImage;
    }
}
