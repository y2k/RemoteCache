using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteCache.Services.Downloader
{
    class WorkerManager
    {
        private const long MaxCacheSize = 20L * 1024 * 1024 * 1024; // 20 GB

        readonly ImageStorage storage;
        readonly DownloadWorker downloader;

        public WorkerManager(ImageStorage storage, DownloadWorker downloader)
        {
            this.storage = storage;
            this.downloader = downloader;
        }

        public void Start()
        {
            storage.Initialize();
            new ClearWorker(storage, MaxCacheSize).Start();
        }

        public Task DownloadImage(Uri source)
        {
            downloader.Execute(source);
        }
    }
}