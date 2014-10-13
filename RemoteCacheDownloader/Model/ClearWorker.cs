using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteCacheDownloader.Model
{
    class ClearWorker
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan SleepTime = new TimeSpan(0, 20, 0); // 20 minutes

        private string cacheRoot;
        private long maxCacheSize;
        private const float trimFactor = 0.8f; // Коэф. размера кэша до которого он уменьшается при привышение лимита.

        public ClearWorker(string cacheRoot, long maxCacheSize)
        {
            this.cacheRoot = cacheRoot;
            this.maxCacheSize = maxCacheSize;
        }

        internal void Start()
        {
            new Thread(() =>
            {
                try
                {
                    Execute();
                }
                catch (Exception e)
                {
                    Log.ErrorException(e.Message, e);
                }

                Thread.Sleep(SleepTime);
            }).Start();
        }

        private void Execute()
        {
            Log.Debug("Start clear");
            var files = Directory.EnumerateFiles(cacheRoot)
                .Where(s => !s.EndsWith("*.tmp"))
                .Select(s => new FileInfo(s))
                .OrderByDescending(s => s.LastWriteTime)
                .ToList();
            Log.Debug("Get all files, count = {0}", files.Count);

            if (files.Sum(s => s.Length) <= maxCacheSize) return; // Если кэш меньши лимита, то выходим

            long total = 0;
            foreach (var f in files)
            {
                total += f.Length;
                if (total > (long)(maxCacheSize * trimFactor))
                {
                    try
                    {
                        f.Delete();
                    }
                    catch (Exception e)
                    {
                        Log.ErrorException("Error while delete file " + f.FullName, e);
                    }
                }
            }

            Log.Debug("End delete file");
        }
    }
}