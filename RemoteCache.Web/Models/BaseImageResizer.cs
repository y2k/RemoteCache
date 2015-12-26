using System;
using System.Drawing;
using System.IO;

namespace RemoteCache.Web.Models
{
    public abstract class BaseImageResizer {
        
        protected Brush background { 
            get {
                if (BackgroundColor == null) return null;
                var color = Convert.ToInt32(BackgroundColor, 16);
                return new SolidBrush(Color.FromArgb(0xFF, Color.FromArgb(color)));
            }
        }
        
        public string BackgroundColor { get; set; }

        public abstract Stream GetRect(int? quality, string imagePath, int width, int height);
    }
}