using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RemoteCache.Services.Downloader
{
    class ImageStorage
    {
        string cacheRoot = Path.Combine(Directory.GetCurrentDirectory(), "cache");

        internal string GetPathForImage(Uri url, string layer = null)
        {
            var md5 = CalculateMD5Hash(url);
            var filename = md5[0] + "/" + md5[1] + md5[2] + "/" + md5.Substring(3) + (layer == null ? "" : "." + layer);
            var path = Path.Combine(cacheRoot, filename);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;
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