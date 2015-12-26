using System;
using System.IO;
using ImageResizer;

namespace RemoteCache.Web.Models 
{    
    class ModernImageResizer : BaseImageResizer {
        
        public override Stream GetRect(int? quality, string imagePath, int width, int height) {
            var buffer = new MemoryStream();
            var normQuality = Math.Max(5, Math.Min(100, quality ?? 90));
            ResizeSettings settings;
            if (BackgroundColor == null) {
                settings = new ResizeSettings($"width={width}&height={height}&mode=crop&quality={normQuality}");
            }else {
                settings = new ResizeSettings($"width={width}&height={height}&mode=crop&quality={normQuality}&bgcolor={BackgroundColor}&format=jpg");
            }
            ImageBuilder.Current.Build(imagePath, buffer, settings);
            buffer.Position = 0;
            return buffer;
        }
    }
}