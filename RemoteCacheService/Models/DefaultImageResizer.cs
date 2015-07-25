using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace RemoteCacheService.Models
{
    class DefaultImageResizer : ImageResizer
    {
        public override Stream GetRect(string imagePath, int width, float minAspect = 1, float maxAspect = 1)
        {
            return Thumbnail(File.OpenRead(imagePath), width, (int)(width / minAspect));
        }

        Stream Thumbnail(Stream source, int width, int maxHeight)
        {
            width = Math.Max(16, Math.Min(1000, width));
            maxHeight = Math.Max(16, Math.Min(1000, maxHeight));

            return CreateThumbnail(
                source,
                image =>
                {
                    int h = (int)Math.Min(maxHeight, ((float)width / image.Width) * image.Height);
                    var thumb = new Bitmap(width, h);
                    using (var g = NewGraphics(thumb))
                    {
                        float s = (float)image.Height / image.Width;
                        g.DrawImage(image, 0, -(thumb.Width * s - thumb.Height) / 2, thumb.Width, thumb.Width * s);
                    }
                    return thumb;
                });
        }

        Stream CreateThumbnail(Stream source, Func<Image, Bitmap> resizeCallback)
        {
            Image thumb;
            ImageFormat f;
            using (var image = Image.FromStream(source))
            {
                f = image.RawFormat;
                thumb = resizeCallback(image);
            }

            // Принудительно конвертируем в jpeg
            if (format == "jpeg" || background != null)
                f = ImageFormat.Jpeg;

            using (var s = new MemoryStream())
            {
                if (f.Guid == ImageFormat.Jpeg.Guid)
                {
                    var enc = ImageCodecInfo.GetImageDecoders().First(i => i.FormatID == ImageFormat.Jpeg.Guid);
                    var p = new EncoderParameters(1);
                    p.Param[0] = new EncoderParameter(Encoder.Quality, 90L);
                    thumb.Save(s, enc, p);
                }
                else
                {
                    thumb.Save(s, f);
                }
                return s;
            }
        }

        Graphics NewGraphics(Bitmap thumb)
        {
            var canvas = Graphics.FromImage(thumb);
            if (background != null)
                canvas.FillRectangle(background, 0, 0, thumb.Width, thumb.Height);
            return canvas;
        }
    }
}