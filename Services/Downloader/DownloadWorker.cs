using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RemoteCache.Services.Downloader
{
    class DownloadWorker
    {
        readonly ImageStorage cacheRoot;
        readonly WorkerManager workPool;

        public DownloadWorker(ImageStorage cacheRoot, WorkerManager workPool)
        {
            this.cacheRoot = cacheRoot;
            this.workPool = workPool;
        }

        public async void Start()
        {
            while (true)
            {
                try { await Execute(); }
                catch (Exception e) { Console.WriteLine(e.Message + "\n" + e); }
            }
        }

        async Task Execute()
        {
            var url = workPool.RegisterNewWork();
            if (url == null) await Task.Delay(1000);
            else
            {
                try { await Execute(url); }
                catch (Exception e) { Console.WriteLine("Can't download " + url + "\n" + e); }
                finally { CompleteTask(url); }
            }
        }

        async Task Execute(Uri uri)
        {
            var target = cacheRoot.GetPathForImage(uri);
            if (File.Exists(target)) return;

            var tmp = cacheRoot.CreateTempFileInCacheDirectory();
            try
            {
                await DownloadTo(uri, tmp);
                if (MediaConverter.Instance.IsCanConvert(tmp))
                {
                    var pathToMp4 = cacheRoot.GetPathForImage(uri, "mp4");
                    await MediaConverter.Instance.ConvertToMp4(tmp, pathToMp4, cacheRoot.CreateTempFileInCacheDirectory());
                }
                File.Move(tmp, target);
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

        void CompleteTask(Uri url) => workPool.UnregisterWork(url);
    }
}