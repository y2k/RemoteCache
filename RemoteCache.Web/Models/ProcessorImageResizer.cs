using System;
using System.Drawing;
using System.IO;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;

namespace RemoteCache.Web.Models
{
    class ProcessorImageResizer : BaseImageResizer {
        
        public override Stream GetRect(int? quality, string imagePath, int width, int height) {
            var factory = new ImageFactory()
                .Load(imagePath);
                
            factory.Format(new JpegFormat());

            factory = factory.Resize(new Size(width, height));                
            
            if (quality != null)
                factory = factory.Quality(Math.Max(5, Math.Min(100, (int)quality)));
                
            if (BackgroundColor != null)
                factory = factory.BackgroundColor(Color.FromName(BackgroundColor));
                
            var buffer = new MemoryStream();
            factory.Save(buffer);
            buffer.Position = 0;
            
            return buffer;
        }
    }
}