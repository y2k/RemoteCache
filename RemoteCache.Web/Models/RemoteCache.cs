using System;
using System.ServiceModel;

namespace RemoteCache.Web.Models
{
    public class RemoteCache
    {
        public string Get(string url, string format)
        {
            return GetImagePathFromRemoteService(url, format);
        }

        string GetImagePathFromRemoteService(string url, string extraLayer = null)
        {
            var factory = new ChannelFactory<IWorkerService>(
                              new BasicHttpBinding(), 
                              new EndpointAddress("http://localhost:8500/remote-cache"));
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