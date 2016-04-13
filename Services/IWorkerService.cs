using System;

namespace RemoteCache.Services
{
    public interface IWorkerService
    {
        string GetPathForImage(Uri url, int preferedWidth, int preferedHeight);

        string GetPathForExtraImage(Uri url, string format);
    }
}