using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using RemoteCache.Services.Downloader;

namespace RemoteCache.Services
{
    public class WorkerService : IWorkerService
    {
        ImageStorage storage = new ImageStorage();
        PreFetcher preFetcher = new PreFetcher();
        WorkerManager workPool = new WorkerManager();

        public WorkerService()
        {
            Console.WriteLine("Program start");
            MediaConverter.Instance.ValidateFFMMPEG();

            workPool.Start();
            Console.WriteLine("Initialize downloaders complete");

            preFetcher.Start();
        }

        public string GetPathForImage(Uri url, int width, int height, HttpRequest request)
        {
            preFetcher.RequestImage(url, width, height, request);
            return Validate(url, storage.GetPathForImage(url));
        }

        public string GetPathForExtraImage(Uri url, string layer)
        {
            return Validate(url, storage.GetPathForImage(url, layer));
        }

        string Validate(Uri source, string file)
        {
            var result = file != null && File.Exists(file) ? file : null;
            if (result == null)
            {
                Console.WriteLine("Get new work " + source);
                workPool.AddWork(source);
            }
            return result;
        }
    }
}