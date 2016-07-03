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

        public DownloadWorker(ImageStorage storage)
        {
            this.storage = storage;
        }

        public async Task DownloadImage(Uri uri)
        {
            using (await downloadLock.Use())
            {
                if (storage.GetPathForImageOrNull(uri) != null) return;

                var tmp = storage.CreateTempFileInCacheDirectory();
                await DownloadTo(uri, tmp);
                await storage.AddFileToStorage(uri, tmp);
            }
        }

        async Task<string> DownloadTo(Uri url, string tmp)
        {
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