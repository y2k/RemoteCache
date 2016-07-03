using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RemoteCache.Common;

namespace RemoteCache.Services.Downloader
{
    class DownloadWorker
    {
        SemaphoreSlim downloadLock = new SemaphoreSlim(5);

        readonly ImageStorage storage;
        readonly MediaConverter mediaConverter;

        public DownloadWorker(ImageStorage cacheRoot, MediaConverter mediaConverter)
        {
            this.storage = cacheRoot;
            this.mediaConverter = mediaConverter;
        }

        public async Task Execute(Uri uri)
        {
            var tmp = storage.CreateTempFileInCacheDirectory();
            await DownloadTo(uri, tmp);
            tmp = await storage.AddFileToStorage(uri, tmp);

            if (mediaConverter.IsCanConvert(tmp))
            {
                var pathToMp4 = storage.CreateTempFileInCacheDirectory();
                await mediaConverter.ConvertToMp4(tmp, pathToMp4, storage.CreateTempFileInCacheDirectory());
                await storage.AddFileToStorage(uri, pathToMp4, "mp4");
            }
        }

        async Task<string> DownloadTo(Uri url, string tmp)
        {
            using (await downloadLock.Use())
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
}