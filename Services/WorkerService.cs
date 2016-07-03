using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RemoteCache.Services.Downloader;

namespace RemoteCache.Services
{
    class WorkerService : IWorkerService
    {
        private const long MaxCacheSize = 20L * 1024 * 1024 * 1024; // 20 GB

        readonly ImageStorage storage;
        readonly PreFetcher preFetcher;
        readonly DownloadWorker downloader;

        public WorkerService(ImageStorage storage, PreFetcher preFetcher, DownloadWorker downloader)
        {
            this.preFetcher = preFetcher;
            this.storage = storage;
            this.downloader = downloader;

            storage.Initialize();
            new ClearWorker(storage, MaxCacheSize).Start();

            preFetcher.Start();
        }

        public Task<string> GetPathForImage(Uri url, int width, int height, HttpRequest request)
        {
            preFetcher.RequestImage(url, width, height, request);
            return DownloadImage(url);
        }

        public Task<string> GetPathForExtraImage(Uri url, string layer)
        {
            return DownloadImage(url, layer);
        }

        async Task<string> DownloadImage(Uri source, string layer = null)
        {
            var file = storage.GetPathForImage(source, layer);
            if (!File.Exists(file))
            {
                Console.WriteLine("Get new work " + source);
                await downloader.Execute(source);
            }
            return File.Exists(file) ? file : null;
        }
    }
}