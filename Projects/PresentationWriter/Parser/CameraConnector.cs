using System;
using HSR.PresentationWriter.DataSources;
using HSR.PresentationWriter.Parser.Events;

namespace HSR.PresentationWriter.Parser
{
    internal class CameraConnector
    {
        public CameraConnector(Camera camera)
        {
        }

        public event EventHandler<NewImageEventArgs> NewImage;
    }
}
