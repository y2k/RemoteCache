using System;
using System.Drawing;
using System.IO;

namespace RemoteCacheModel
{
    public abstract class ImageResizer
    {
        protected Brush background { get; set; }

        public string format { get; set; }

        public void SetJpegBackground(string hexColor)
        {
            var color = Convert.ToInt32(hexColor, 16);
            background = new SolidBrush(Color.FromArgb(0xFF, Color.FromArgb(color)));
        }

        public abstract Stream GetRect(string imagePath, int width, float minAspect = 1, float maxAspect = 1);
    }
}