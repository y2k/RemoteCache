using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.ServiceModel;
using RemoteCacheDownloader.Service;

namespace RemoteCacheService.Models
{
    public class CacheModel
    {
        public string Get(string url)
        {
//            var factory = new ChannelFactory<IWorkerService>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:8192/remote-cache"));
            var factory = new ChannelFactory<IWorkerService>(new BasicHttpBinding(), new EndpointAddress("http://localhost:8192/remote-cache"));
            var client = factory.CreateChannel();
            try
            {
                var file = client.GetPathForImage(new Uri(url));
                if (file == null) throw new Exception();
                return file;
            }
            catch
            {
                client.AddWork(new Uri(url));
            }
            return null;
        }

        public byte[] Square(string url, int size, string format)
        {
            return CreateThumbnail(url, format, image =>
                {
                    int w = Math.Min(size, Math.Min(image.Width, image.Height));
                    var thumb = new Bitmap(w, w);
                    using (var g = Graphics.FromImage(thumb))
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

            return CreateThumbnail(url, format, image =>
                {
                    int h = (int)Math.Min(maxHeight, ((float)width / image.Width) * image.Height);
                    var thumb = new Bitmap(width, h);
                    using (var g = Graphics.FromImage(thumb))
                    {
                        float s = (float)image.Height / image.Width;
                        g.DrawImage(image, 0, -(thumb.Width * s - thumb.Height) / 2, thumb.Width, thumb.Width * s);
                    }
                    return thumb;
                });
        }

        byte[] CreateThumbnail(String url, String format, Func<Image, Bitmap> resizeCallback)
        {
            var src = Get(url);
            if (src == null)
                return null;

            Image thumb;
            ImageFormat f;
            using (var image = Bitmap.FromFile(src))
            {
                f = image.RawFormat;
                thumb = resizeCallback(image);
            }

            // Принудительно конвертируем в jpeg
            if (format == "jpeg")
                f = ImageFormat.Jpeg;

            using (var s = new MemoryStream())
            {
                if (f.Guid == ImageFormat.Jpeg.Guid)
                {
                    var enc = ImageCodecInfo.GetImageDecoders().First(i => i.FormatID == ImageFormat.Jpeg.Guid);
                    var p = new EncoderParameters(1);
                    p.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
                    thumb.Save(s, enc, p);
                }
                else
                    thumb.Save(s, f);

                return s.ToArray();
            }
        }
    }
}