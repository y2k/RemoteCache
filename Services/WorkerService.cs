using System;
using System.IO;
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

        public void AddWork(Uri source)
        {
            Console.WriteLine("Get new work " + source);
            workPool.AddWork(source);
        }

        public string GetPathForImage(Uri url, int width, int height)
        {
            preFetcher.RequestImage(url, width, height);
            return Validate(storage.GetPathForImage(url));
        }

        public string GetPathForExtraImage(Uri url, string layer)
        {
            return Validate(storage.GetPathForImage(url, layer));
        }

        static string Validate(string file)
        {
            return file != null && File.Exists(file) ? file : null;
        }
    }
}