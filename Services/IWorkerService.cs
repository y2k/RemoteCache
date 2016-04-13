using System;

namespace RemoteCache.Services
{
    public interface IWorkerService
    {
        string GetPathForImage(Uri url, int preferedWidth, int preferedHeight, Uri requestUri);

        string GetPathForExtraImage(Uri url, string format);
    }
}