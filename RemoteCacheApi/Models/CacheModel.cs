using System;
using System.ServiceModel;

namespace RemoteCacheApi.Models
{
    public class CacheModel
    {
        internal byte[] Square(string url, int value, string format)
        {
            throw new NotImplementedException();
        }

        internal byte[] Thumbnail(string url, int value1, int value2, string format)
        {
            throw new NotImplementedException();
        }

        internal string Get(string url)
        {
            var factory = new ChannelFactory<IWorkerService>(new BasicHttpBinding(), new EndpointAddress("http://localhost:8192/remote-cache"));
            var client = factory.CreateChannel();
            try
            {
                var file = client.GetPathForImage(new Uri(url));
                if (file == null) throw new Exception();
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