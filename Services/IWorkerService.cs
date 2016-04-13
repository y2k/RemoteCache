using System;

namespace RemoteCache.Services
{
    public interface IWorkerService
    {
        void AddWork(Uri source);

        string GetPathForImage(Uri url, int preferedWidth, int preferedHeight);

        string GetPathForExtraImage(Uri url, string format);
    }
}