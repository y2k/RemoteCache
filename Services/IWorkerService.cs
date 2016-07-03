using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RemoteCache.Services
{
    public interface IWorkerService
    {
        Task<string> GetPathForImage(Uri url, int preferedWidth, int preferedHeight, HttpRequest request);

        Task<string> GetPathForExtraImage(Uri url, string format);
    }
}