using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.ServiceModel;
using RemoteCacheDownloader.Service;

namespace RemoteCacheService.Models
{
    class RemoteCache
    {
        Brush background;

        public void SetJpegBackground(string hexColor)
        {
            var color = Convert.ToInt32(hexColor, 16);
            background = new SolidBrush(Color.FromArgb(0xFF, Color.FromArgb(color)));
        }

        public string Get(string url, string format)
        {
            return GetImagePathFromRemoteService(url, format);
        }

        public byte[] Square(string url, int size, string format)
        {
            return CreateThumbnail(url, format,
                image =>
                {
                    int w = Math.Min(size, Math.Min(image.Width, image.Height));
                    var thumb = new Bitmap(w, w);
                    using (var g = NewGraphics(thumb))
                    {
                        float s = (float)w / Math.Min(image.Width, image.Height);
                        g.DrawImage(image, (w - image.Width * s) / 2, (w - image.Height * s) / 2, image.Width * s, image.Height * s);
                    }
                    return thumb;
                });
        }

        public byte[] Thumbnail(string url, int width, int maxHeight, string format)
        {
            width = Math.Max(16, Math.Min(1000, width));
            maxHeight = Math.Max(16, Math.Min(1000, maxHeight));

            return CreateThumbnail(url, format,
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

        byte[] CreateThumbnail(string url, string format, Func<Image, Bitmap> resizeCallback)
        {
            var src = GetImagePathFromRemoteService(url);
            if (src == null)
                return null;

            Image thumb;
            ImageFormat f;
            using (var image = Image.FromFile(src))
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
                return s.ToArray();
            }
        }

        string GetImagePathFromRemoteService(string url, string extraLayer = null)
        {
            var factory = new ChannelFactory<IWorkerService>(
                              new BasicHttpBinding(), 
                              new EndpointAddress("http://localhost:8192/remote-cache"));
            var client = factory.CreateChannel();
            try
            {
                var file = extraLayer == null
                    ? client.GetPathForImage(new Uri(url))
                    : client.GetPathForExtraImage(new Uri(url), extraLayer);
                if (file == null)
                    throw new Exception();
                return file;
            }
            catch
            {
                client.AddWork(new Uri(url));
            }
            return null;
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