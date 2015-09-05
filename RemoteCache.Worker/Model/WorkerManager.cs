﻿using System;
using System.Collections.Generic;

namespace RemoteCache.Worker.Model
{
    class WorkerManager
    {
        const int ImagePerChunk = 100;
        const int MaxThreads = 20;

        #if DEBUG
        private const long MaxCacheSize = 512 * 1024 * 1024;
        // 20 GB
        #else
        private const long MaxCacheSize = 20L * 1024 * 1024 * 1024; // 20 GB
#endif

        WorkerManager()
        {
        }

        public static readonly WorkerManager Instance = new WorkerManager();

        readonly Stack<Uri> DownloadUrls = new Stack<Uri>();
        readonly HashSet<Uri> LockedUrls = new HashSet<Uri>();

        public void Start()
        {
            var storage = new ImageStorage();
            storage.Initialize();

            new ClearWorker(storage, MaxCacheSize).Start();
            for (int i = 0; i < MaxThreads; i++)
                new DownloadWorker(storage).Start();
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