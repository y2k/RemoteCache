using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using RemoteCache.Worker.Model;

namespace RemoteCache.Worker.Service
{
    public class WorkerService : IWorkerService
    {
        ImageStorage storage = new ImageStorage();
        PreFetcher preFetcher = Program.PreFetcher;

        public void AddWork(Uri source)
        {
            Console.WriteLine("Get new work " + source);
            WorkerManager.Instance.AddWork(source);
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

        public static void InitializeService()
        {
            var host = new ServiceHost(typeof(WorkerService), new Uri("http://localhost:8500/remote-cache"));
            host.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = false });
            host.Open();
        }
    }
}