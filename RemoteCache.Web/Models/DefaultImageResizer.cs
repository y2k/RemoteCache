using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace RemoteCache.Web.Models
{
    public class DefaultImageResizer : BaseImageResizer
    {
        public override Stream GetRect(int? quality, string imagePath, int width, float minAspect = 1, float maxAspect = 1)
        {
            Image thumb;
            ImageFormat f;

            using (var source = File.OpenRead(imagePath))
            {
                using (var image = Image.FromStream(source))
                {
                    f = image.RawFormat;
                    thumb = Convert(image, NormalizeWidth(width), minAspect, maxAspect);
                }
            }

            return Encode(thumb, ref f, quality);
        }

        private static int NormalizeWidth(int width)
        {
            return Math.Max(10, Math.Min(1000, width));
        }

        Bitmap Convert(Image image, int width, float minAspect, float maxAspect)
        {
            var fh = (float)width / image.Width * image.Height;
            var minH = width / maxAspect;
            var maxH = width / minAspect;

            var height = (int)Math.Max(minH, Math.Min(maxH, fh));

            var thumb = new Bitmap(width, height);
            using (var g = NewGraphics(thumb))
            {
                float s = (float)image.Height / image.Width;
                var rect = new RectangleF(0, -(thumb.Width * s - thumb.Height) / 2, thumb.Width, thumb.Width * s);
                if (rect.Height < thumb.Height)
                {
                    var correctScale = thumb.Height / rect.Height;
                    rect.Height *= correctScale;
                    rect.Width *= correctScale;
                    rect.X = (thumb.Width - rect.Width) / 2;
                    rect.Y = 0;
                }
                g.DrawImage(image, rect);
            }
            return thumb;
        }

        Graphics NewGraphics(Bitmap thumb)
        {
            var canvas = Graphics.FromImage(thumb);
            canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
            if (background != null)
                canvas.FillRectangle(background, 0, 0, thumb.Width, thumb.Height);
            return canvas;
        }

        Stream Encode(Image thumb, ref ImageFormat f, int? quality)
        {
            // Принудительно конвертируем в jpeg
            if (format == "jpeg" || background != null)
                f = ImageFormat.Jpeg;

            var buffer = new MemoryStream();
            if (f.Guid == ImageFormat.Jpeg.Guid)
            {
                var enc = ImageCodecInfo.GetImageDecoders().First(i => i.FormatID == ImageFormat.Jpeg.Guid);
                var p = new EncoderParameters(1);
                p.Param[0] = new EncoderParameter(Encoder.Quality, (long)Math.Max(5, Math.Min(100, quality ?? 90)));
                thumb.Save(buffer, enc, p);
            }
            else
            {
                thumb.Save(buffer, f);
            }

            buffer.Position = 0;
            return buffer;
        }
    }
}
