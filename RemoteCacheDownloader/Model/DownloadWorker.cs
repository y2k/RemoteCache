using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteCacheDownloader.Model
{
    class DownloadWorker
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private string cacheRoot;

        public DownloadWorker(string cacheRoot)
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
                        Log.ErrorException(e.Message, e);
                    }
                }
            }).Start();
        }

        private void Execute()
        {
            var url = CreateTask();

            if (url == null) Thread.Sleep(1000);
            else
            {
                try
                {
                    DownloadToFile(url, cacheRoot);
                }
                catch (Exception e)
                {
                    Log.WarnException("Can't download " + url, e);
                }
                finally
                {
                    CompleteTask(url);
                }
            }
        }

        private static Uri CreateTask()
        {
            return WorkerManager.Instance.RegisterNewWork();
        }

        private static void DownloadToFile(Uri url, string cacheRoot)
        {
            var target = Path.Combine(cacheRoot, CalculateMD5Hash("" + url));
            if (File.Exists(target)) return;

            var tmp = Path.Combine(cacheRoot, Guid.NewGuid() + ".tmp");
            Log.Trace("Download url {0} -> {1}", url, tmp);

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Referer = url.AbsoluteUri;
#if USER_PROXY
            req.Proxy = new WebProxy("127.0.0.1:8118");
#endif
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.76 Safari/537.36 OPR/16.0.1196.80";

            var resp = (HttpWebResponse)req.GetResponse();

            var i = resp.GetResponseStream();
            var o = new FileStream(tmp, FileMode.Create);
            byte[] buf = new byte[4 * 1024];
            int count;

            while ((count = i.Read(buf, 0, buf.Length)) > 0)
            {
                o.Write(buf, 0, count);
            }

            i.Close();
            o.Close();

            File.Move(tmp, target);
        }

        private static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        private static void CompleteTask(Uri url)
        {
            WorkerManager.Instance.UnregisterWork(url);
        }
    }
}