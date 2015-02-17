using System;
using System.Collections.Generic;

namespace RemoteCacheDownloader.Model
{
    class WorkerManager
    {
        private static readonly TimeSpan WaitPeriod = new TimeSpan(0, 0, 1);

        private const int ImagePerChunk = 100;
        private const int MaxThreads = 20;

#if DEBUG
        private const long MaxCacheSize = 512 * 1024 * 1024; // 20 GB
#else
        private const long MaxCacheSize = 20L * 1024 * 1024 * 1024; // 20 GB
#endif

        private WorkerManager() { }

        public static readonly WorkerManager Instance = new WorkerManager();

        private readonly Stack<Uri> DownloadUrls = new Stack<Uri>();
        private readonly HashSet<Uri> LockedUrls = new HashSet<Uri>();

        public void Start()
        {
            var storage = new ImageStorage();
            storage.Initialize();

            new ClearWorker(storage, MaxCacheSize).Start();
            for (int i = 0; i < MaxThreads; i++) new DownloadWorker(storage).Start();
        }

        public void AddWork(Uri source)
        {
            lock (this)
            {
                DownloadUrls.Push(source);
            }
        }

        internal string GetPathForImage(Uri source)
        {
            lock (this)
            {
                throw new NotImplementedException();
            }
        }

        public Uri RegisterNewWork()
        {
            lock (this)
            {
                while (DownloadUrls.Count > 0)
                {
                    var url = DownloadUrls.Pop();
                    if (LockedUrls.Add(url)) return url;
                }
                return null;
            }
        }

        public void UnregisterWork(Uri url)
        {
            lock (this)
            {
                LockedUrls.Remove(url);
            }
        }
    }
}