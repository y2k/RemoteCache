using System;
using System.IO;
using System.Net;
using System.Threading;

namespace RemoteCache.Worker.Model
{
    class DownloadWorker
    {
        ImageStorage cacheRoot;

        public DownloadWorker(ImageStorage cacheRoot)
        {
            this.cacheRoot = cacheRoot;
        }

        public void Start()
        {
            new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            Execute();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message + "\n" + e);
                        }
                    }
                }).Start();
        }

        void Execute()
        {
            var url = WorkerManager.Instance.RegisterNewWork();

            if (url == null)
                Thread.Sleep(1000);
            else
            {
                try
                {
                    Execute(url);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Can't download " + url + "\n" + e);
                }
                finally
                {
                    CompleteTask(url);
                }
            }
        }

        void Execute(Uri uri)
        {
            var target = cacheRoot.GetPathForImage(uri);
            if (File.Exists(target))
                return;

            var tmp = cacheRoot.CreateTempFileInCacheDirectory();
            try
            {
                DownloadTo(uri, tmp);
                if (GifConverter.Instance.IsCanConvert(tmp))
                {
                    var pathToMp4 = cacheRoot.GetPathForImage(uri, "mp4");
                    GifConverter.Instance.ConvertToMp4(tmp, pathToMp4, cacheRoot.CreateTempFileInCacheDirectory());
                }
                File.Move(tmp, target);
            }
            finally
            {
                File.Delete(tmp);
            }
        }

        string DownloadTo(Uri url, string tmp)
        {
            Console.WriteLine("Download url {0} -> {1}", url, tmp);

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Referer = url.AbsoluteUri;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.76 Safari/537.36 OPR/16.0.1196.80";

            var resp = (HttpWebResponse)req.GetResponse();
            using (var i = resp.GetResponseStream())
            using (var o = new FileStream(tmp, FileMode.Create))
                i.CopyTo(o);
            return tmp;
        }

        void CompleteTask(Uri url)
        {
            WorkerManager.Instance.UnregisterWork(url);
        }
    }
}