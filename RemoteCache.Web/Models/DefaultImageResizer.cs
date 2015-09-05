using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace RemoteCache.Web.Models
{
    public class DefaultImageResizer : BaseImageResizer
    {
        public override Stream GetRect(string imagePath, int width, float minAspect = 1, float maxAspect = 1)
        {
            Image thumb;
            ImageFormat f;

            using (var source = File.OpenRead(imagePath))
            {
                using (var image = Image.FromStream(source))
                {
                    f = image.RawFormat;
                    thumb = Convert(image, width, minAspect, maxAspect);
                }
            }

            return Encode(thumb, ref f);
        }

        Bitmap Convert(Image image, int width, float minAspect, float maxAspect)
        {
            var fh = (float)width / image.Width * image.Height;
            var minH = width * minAspect;
            var maxH = width * maxAspect;

            var height = (int)Math.Max(minH, Math.Min(maxH, fh));

            var thumb = new Bitmap(width, height);
            using (var g = NewGraphics(thumb))
            {
                float s = (float)image.Height / image.Width;
                g.DrawImage(image, 0, -(thumb.Width * s - thumb.Height) / 2, thumb.Width, thumb.Width * s);
            }
            return thumb;
        }

        Graphics NewGraphics(Bitmap thumb)
        {
            var canvas = Graphics.FromImage(thumb);
            if (background != null)
                canvas.FillRectangle(background, 0, 0, thumb.Width, thumb.Height);
            return canvas;
        }

        Stream Encode(Image thumb, ref ImageFormat f)
        {
            // Принудительно конвертируем в jpeg
            if (format == "jpeg" || background != null)
                f = ImageFormat.Jpeg;

            var buffer = new MemoryStream();
            if (f.Guid == ImageFormat.Jpeg.Guid)
            {
                var enc = ImageCodecInfo.GetImageDecoders().First(i => i.FormatID == ImageFormat.Jpeg.Guid);
                var p = new EncoderParameters(1);
                p.Param[0] = new EncoderParameter(Encoder.Quality, 90L);
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