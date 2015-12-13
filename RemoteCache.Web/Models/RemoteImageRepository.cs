using System;
using System.ServiceModel;

namespace RemoteCache.Web.Models
{
    public class RemoteImageRepository
    {
        public string Get(string url, string format)
        {
            return GetImagePathFromRemoteService(url, format);
        }

        string GetImagePathFromRemoteService(string url, string extraLayer = null)
        {
            var workerHost = Environment.GetEnvironmentVariable("WORKER_PORT_8500_TCP_ADDR") ?? "localhost";
            var factory = new ChannelFactory<IWorkerService>(
                              new BasicHttpBinding(), 
                              new EndpointAddress("http://" + workerHost + ":8500/remote-cache"));
            var client = factory.CreateChannel();
            try
            {
                var file = extraLayer == null
                    ? client.GetPathForImage(new Uri(url))
                    : client.GetPathForExtraImage(new Uri(url), extraLayer);
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