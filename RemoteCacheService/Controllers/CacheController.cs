using RemoteCacheModel;
using System;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace RemoteCacheService.Controllers
{
    public class CacheController : Controller
    {
        RemoteCache imageRepository = new RemoteCache();
        ImageResizer resizer = new DefaultImageResizer();

        public ActionResult Get(string url, string format, string bgColor, int? width = null)
        {
            Uri tmp;
            if (!Uri.TryCreate(url, UriKind.Absolute, out tmp))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var path = imageRepository.Get(url, format);
            if (path == null)
            {
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            if (width.HasValue)
            {
                if (bgColor != null)
                    resizer.SetJpegBackground(bgColor);

                var result = resizer.GetRect(path, (int)width, 0.5f, 2);
                ConfigureCache();
                return File(result, "image/jpeg");
            }
            else
            {
                ConfigureCache();
                return File(path, "mp4" == format ? "video/mp4" : "image/jpeg");
            }
        }

        void ConfigureCache()
        {
#if !DEBUG
            var cache = Response.Cache;
            cache.SetCacheability(HttpCacheability.Public);
            cache.SetExpires(new DateTime(2525, 1, 1));
#endif
        }
    }
}