using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GFace
{
    class CameraState
    {
        private int interval;
        private int exposure;
        private int frameCount = 0;

        private bool isRunning = false;
        private bool isRenderingFaces = false;

        public bool IsRunning { get => isRunning; set => isRunning = value; }
        public bool IsRenderingFaces { get => isRenderingFaces; set => isRenderingFaces = value; }
        public int Exposure { get => exposure; set => exposure = value; }
        public int Interval { get => interval; set => interval = value; }
        public int FrameCount { get => frameCount; set => frameCount = value; }

        public CameraState()
        {
            // this.camera = new VideoCapture();
        }


    }
}
