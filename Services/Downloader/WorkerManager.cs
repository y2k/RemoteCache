using System;
using System.Collections.Generic;

namespace RemoteCache.Services.Downloader
{
    class WorkerManager
    {
        const int MaxThreads = 10;
        private const long MaxCacheSize = 20L * 1024 * 1024 * 1024; // 20 GB

        ImageStorage storage = new ImageStorage();

        readonly Stack<Uri> DownloadUrls = new Stack<Uri>();
        readonly HashSet<Uri> LockedUrls = new HashSet<Uri>();

        public void Start()
        {
            storage.Initialize();

            new ClearWorker(storage, MaxCacheSize).Start();
            for (int i = 0; i < MaxThreads; i++)
                new DownloadWorker(storage, this).Start();
        }

        readonly object locker = new object();

        public void AddWork(Uri source)
        {
            lock (locker)
            {
                DownloadUrls.Push(source);
            }
        }

        public Uri RegisterNewWork()
        {
            lock (locker)
            {
                while (DownloadUrls.Count > 0)
                {
                    var url = DownloadUrls.Pop();
                    if (LockedUrls.Add(url))
                        return url;
                }
                return null;
            }
        }

        public void UnregisterWork(Uri url)
        {
            lock (locker)
            {
                LockedUrls.Remove(url);
            }
        }
    }
}