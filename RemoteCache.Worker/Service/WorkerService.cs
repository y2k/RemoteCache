using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using RemoteCache.Worker.Model;

namespace RemoteCache.Worker.Service
{
    public class WorkerService : IWorkerService
    {
        public void AddWork(Uri source)
        {
            Console.WriteLine("Get new work " + source);
            WorkerManager.Instance.AddWork(source);
        }

        public string GetPathForImage(Uri url)
        {
            return GetLayer(url, null);
        }

        public string GetPathForExtraImage(Uri url, string layer)
        {
            return GetLayer(url, layer);
        }

        string GetLayer(Uri url, string layer)
        {
            var file = new ImageStorage().GetPathForImage(url, layer);
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