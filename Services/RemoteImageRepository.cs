using System;

namespace RemoteCache.Services
{
    public class RemoteImageRepository
    {
        IWorkerService client;

        public RemoteImageRepository(IWorkerService client)
        {
            this.client = client;
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
                var file = width != 0 && height != 0
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