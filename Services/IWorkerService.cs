using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RemoteCache.Services
{
    public interface IWorkerService
    {
        string GetPathForImage(Uri url, int preferedWidth, int preferedHeight, HttpRequest request);

        Task<string> GetPathForExtraImageAsync(Uri url, string format);
    }
}