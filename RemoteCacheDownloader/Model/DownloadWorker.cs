using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace RemoteCacheDownloader.Model
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

        void Execute(Uri url)
        {
            var target = cacheRoot.GetPathForImage(url);
            if (File.Exists(target))
                return;

            var tmp = DownloadToTemp(url);
            if (url.AbsoluteUri.EndsWith(".gif"))
                ConvertToMp4(tmp, url);

            File.Move(tmp, target);
        }

        string DownloadToTemp(Uri url)
        {
            var tmp = cacheRoot.CreateTempFileInCacheDirectory();
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

        void ConvertToMp4(string tmp, Uri url)
        {
            const string args = "-i {0} -vprofile baseline -vcodec libx264 -acodec aac -strict -2 -g 30 -pix_fmt yuv420p -vf \"scale=trunc(in_w/2)*2:trunc(in_h/2)*2\" {1}";
            var mp4Temp = cacheRoot.CreateTempFileInCacheDirectory() + ".mp4";

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = GetFFFMPEG(),
                    Arguments = string.Format(args, tmp, mp4Temp)
                }).WaitForExit();

            File.Move(mp4Temp, cacheRoot.GetPathForImage(ConvertGifUriToMp4(url)));
        }

        public static string GetFFFMPEG()
        {
            var path = Environment.GetEnvironmentVariable("REMOTECACHE_FFMPEG_DIR");
            if (path == null)
                throw new Exception("Env 'REMOTECACHE_FFMPEG_DIR' not found");
            return Path.Combine(path, "ffmpeg");
        }

        Uri ConvertGifUriToMp4(Uri gifUri)
        {
            return new Uri(gifUri.AbsoluteUri + ".mp4");
        }

        void CompleteTask(Uri url)
        {
            WorkerManager.Instance.UnregisterWork(url);
        }
    }
}