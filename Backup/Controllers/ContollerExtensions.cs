using System.Web;

namespace RemoteCacheService.Controllers
{
    static class ContollerExtensions
    {
        public static int? AsInt(this HttpRequestBase request, string key)
        {
            return request[key] == null ? (int?)null : int.Parse(request[key]);
        }
    }
}