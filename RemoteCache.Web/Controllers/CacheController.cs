using Microsoft.AspNet.Mvc;
using RemoteCache.Web.Models;
using System;
using System.Net;

namespace RemoteCache.Web.Controllers
{
    public class CacheController : Controller
    {
        RemoteImageRepository imageRepository = new RemoteImageRepository();
        BaseImageResizer resizer = new DefaultImageResizer();

        public ActionResult Get(string url, string format, string bgColor, int? width = null)
        {
            Uri tmp;
            if (!Uri.TryCreate(url, UriKind.Absolute, out tmp))
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);

            var path = imageRepository.Get(url, format);
            if (path == null)
            {
                //  Response.Cache.SetCacheability(HttpCacheability.NoCache);
                return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);
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
//  #if !DEBUG
//              var cache = Response.Cache;
//              cache.SetCacheability(HttpCacheability.Public);
//              cache.SetExpires(new DateTime(2525, 1, 1));
//  #endif
        }
    }
}