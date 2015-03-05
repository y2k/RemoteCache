using ImageResizer;
using System;
using System.IO;
using System.Linq;
using System.ServiceModel;

namespace RemoteCacheApi.Models
{
    public class CacheModel
    {
        public string Get(string url)
        {
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

        public Stream Square(string url, int size, string format)
        {
            var inStream = OpenImageFromDisk(url);
            if (inStream == null) return null;

            var result = new MemoryStream();
            var job = new ImageJob(
                inStream,
                result,
                new Instructions($"mode=crop;format={format ?? "jpg"};width={size};height={size};"));
            job.CreateParentDirectory = true;
            job.AddFileExtension = true;
            job.Build();
            result.Position = 0;
            return result;

        }

        private Stream OpenImageFromDisk(string url)
        {
            var path = Get(url);
            return path == null ? null : new FileStream(path, FileMode.Open);
        }

        public byte[] Thumbnail(string url, int width, int maxHeight, string format)
        {
            //width = Math.Max(16, Math.Min(1000, width));
            //maxHeight = Math.Max(16, Math.Min(1000, maxHeight));

            //return CreateThumbnail(url, format, image =>
            //{
            //    int h = (int)Math.Min(maxHeight, ((float)width / image.Width) * image.Height);
            //    var thumb = new Bitmap(width, h);
            //    using (var g = Graphics.FromImage(thumb))
            //    {
            //        float s = (float)image.Height / image.Width;
            //        g.DrawImage(image, 0, -(thumb.Width * s - thumb.Height) / 2, thumb.Width, thumb.Width * s);
            //    }
            //    return thumb;
            //});
            //return null;
            throw new NotImplementedException();
        }
    }
}