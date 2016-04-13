using System;
using System.IO;

namespace RemoteCache.Web.Models
{
    public abstract class BaseImageResizer {
        
        protected Color background { 
            get {
                if (BackgroundColor == null) return null;
                var color = Convert.ToInt32(BackgroundColor, 16);
                return new Color(0xFF, color);
            }
        }
        
        public string BackgroundColor { get; set; }

        public abstract Stream GetRect(int? quality, string imagePath, int width, int height);
    }
}