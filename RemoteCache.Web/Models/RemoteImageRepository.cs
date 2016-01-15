using System;
using System.ServiceModel;

namespace RemoteCache.Web.Models
{
    public class RemoteImageRepository
    {
        IWorkerService client;

        public RemoteImageRepository()
        {
            var workerHost = Environment.GetEnvironmentVariable("WORKER_PORT_8500_TCP_ADDR") ?? "localhost";
            var factory = new ChannelFactory<IWorkerService>(
                              new BasicHttpBinding(),
                              new EndpointAddress("http://" + workerHost + ":8500/remote-cache"));
            client = factory.CreateChannel();
        }

        public string Get(string url, int width, int height)
        {
            return GetImagePathFromRemoteService(url, width, height);
        }

        public string Get(string url, string format)
        {
            return GetImagePathFromRemoteService(url, 0, 0, format);
        }

        string GetImagePathFromRemoteService(string url, int width, int height, string extraLayer = null)
        {
            try
            {
                var file = extraLayer == null
                    ? client.GetPathForImage(new Uri(url), width, height)
                    : client.GetPathForExtraImage(new Uri(url), extraLayer);

                Console.WriteLine($"{url} -> {file}");

                if (file == null)
                    throw new Exception();
                return file;
            }
            catch
            {
                client.AddWork(new Uri(url));
            }
            return null;
        }
    }
}