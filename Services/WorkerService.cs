using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RemoteCache.Services.Downloader;

namespace RemoteCache.Services
{
    class WorkerService : IWorkerService
    {
        readonly WorkerManager workPool;
        readonly ImageStorage storage;
        readonly PreFetcher preFetcher;

        public WorkerService(ImageStorage storage, PreFetcher preFetcher, WorkerManager workPool)
        {
            this.preFetcher = preFetcher;
            this.storage = storage;
            this.workPool = workPool;

            Console.WriteLine("Program start");
            MediaConverter.Instance.ValidateFFMMPEG();

            workPool.Start();
            Console.WriteLine("Initialize downloaders complete");

            preFetcher.Start();
        }

        public Task<string> GetPathForImage(Uri url, int width, int height, HttpRequest request)
        {
            preFetcher.RequestImage(url, width, height, request);
            return DownloadImage(url, storage.GetPathForImage(url));
        }

        public Task<string> GetPathForExtraImage(Uri url, string layer)
        {
            return DownloadImage(url, storage.GetPathForImage(url, layer));
        }

        async Task<string> DownloadImage(Uri source, string file)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine("Get new work " + source);
                await workPool.DownloadImage(source);
            }
            return File.Exists(file) ? file : null;
        }
    }
}