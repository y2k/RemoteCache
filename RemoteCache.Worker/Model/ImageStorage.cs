using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RemoteCache.Worker.Model
{
    class ImageStorage
    {
        string cacheRoot = Path.Combine(Directory.GetCurrentDirectory(), "Cache");
        SizeSelector sizeSelector;
        ImageMeta imageMeta;

        public ImageStorage(SizeSelector sizeSelector, ImageMeta imageMeta)
        {
            this.sizeSelector = sizeSelector;
            this.imageMeta = imageMeta;
        }

        internal string GetThubmnail(Uri url, Size size)
        {
            var originalSize = imageMeta.Get(GetPathForImage(url));
            var best = sizeSelector.GetBest(originalSize, size);

            Console.WriteLine("LOG | " + size + " -> " + best);

            var md5 = CalculateMD5Hash(url);
            var filename = $"{md5}.thumb.{best.width}x{best.height}";
            return Path.Combine(cacheRoot, filename);
        }

        internal string GetPathForImage(Uri url, string layer = null)
        {
            var md5 = CalculateMD5Hash(url);
            var filename = md5 + (layer == null ? "" : "." + layer);
            return Path.Combine(cacheRoot, filename);
        }

        internal void Initialize()
        {
            Directory.CreateDirectory(cacheRoot);

            Console.WriteLine("START clear temp files");
            foreach (var f in Directory.GetFiles(cacheRoot, "*.tmp"))
                File.Delete(f);
            Console.WriteLine("END clear temp files");
        }

        internal string GetRootDirectory()
        {
            return cacheRoot;
        }

        static string CalculateMD5Hash(Uri url)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(url.AbsoluteUri);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        internal string CreateTempFileInCacheDirectory()
        {
            return Path.Combine(cacheRoot, Guid.NewGuid() + ".tmp");
        }
    }
}