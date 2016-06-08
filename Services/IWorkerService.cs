using System;
using Microsoft.AspNetCore.Http;

namespace RemoteCache.Services
{
    public interface IWorkerService
    {
        string GetPathForImage(Uri url, int preferedWidth, int preferedHeight, HttpRequest request);

        string GetPathForExtraImage(Uri url, string format);
    }
}