using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RemoteCache.Services.Downloader
{
    class DownloadWorker
    {
        readonly ImageStorage storage;

        public DownloadWorker(ImageStorage cacheRoot)
        {
            this.storage = cacheRoot;
        }

        public async Task Execute(Uri uri)
        {
            var tmp = storage.CreateTempFileInCacheDirectory();
            try
            {
                await DownloadTo(uri, tmp);
                string pathToMp4 = null;
                if (MediaConverter.Instance.IsCanConvert(tmp))
                {
                    pathToMp4 = storage.CreateTempFileInCacheDirectory();
                    await MediaConverter.Instance.ConvertToMp4(tmp, pathToMp4, storage.CreateTempFileInCacheDirectory());
                }

                await storage.AddFileToStorage(uri, tmp, pathToMp4);
            }
            finally { File.Delete(tmp); }
        }

        async Task<string> DownloadTo(Uri url, string tmp)
        {
            Console.WriteLine("Download url {0} -> {1}", url, tmp);

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Headers["Referer"] = url.AbsoluteUri;
            req.Headers["UserAgent"] = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.76 Safari/537.36 OPR/16.0.1196.80";

            var resp = (HttpWebResponse)await req.GetResponseAsync();
            using (var i = resp.GetResponseStream())
            using (var o = new FileStream(tmp, FileMode.Create))
                await i.CopyToAsync(o);
            return tmp;
        }
    }
}