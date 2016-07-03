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
        readonly MediaConverter mediaConverter;

        public WorkerService(ImageStorage storage, PreFetcher preFetcher, DownloadWorker downloader, MediaConverter mediaConverter)
        {
            this.preFetcher = preFetcher;
            this.storage = storage;
            this.downloader = downloader;
            this.mediaConverter = mediaConverter;

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
            if (storage.GetPathForImageOrNull(source, layer) == null)
            {
                await downloader.DownloadImage(source);

                if (layer == null) mediaConverter.ConvertGifToMp4IgnoreResule(source);
                else await mediaConverter.ConvertGifToMp4(source);
            }

            return storage.GetPathForImageOrNull(source, layer);
        }
    }
}