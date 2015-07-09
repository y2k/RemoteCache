using System;
using System.Net;
using Microsoft.AspNet.Mvc;
using RemoteCacheApi.Models;

namespace RemoteCacheApi.Controllers
{
    public class CacheController : Controller
    {
        CacheModel model = new CacheModel();

        public ActionResult Test()
        {
            return Content("test");
        }

        public ActionResult Get(string url, string format, int? size = null, int? width = null, int? maxHeight = null)
        {
            Uri tmp;
            if (!Uri.TryCreate(url, UriKind.Absolute, out tmp))
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            if (size.HasValue && (size < 16 || size > 512))
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);

            if (size.HasValue)
            {
                var data = model.Square(url, size.Value, format);
                if (data != null)
                {
#if !DEBUG
                    var cache = Response.Cache;
                    cache.SetCacheability(HttpCacheability.Public);
                    cache.SetExpires(new DateTime(2525, 1, 1));
#endif
                    return new FileStreamResult(data, "image/jpeg");
                }
            }
            else if (width.HasValue && maxHeight.HasValue)
            {
                var data = model.Thumbnail(url, width.Value, maxHeight.Value, format);
                if (data != null)
                {
#if !DEBUG
                    var cache = Response.Cache;
                    cache.SetCacheability(HttpCacheability.Public);
                    cache.SetExpires(new DateTime(2525, 1, 1));
#endif
                    return new FileContentResult(data, "image/jpeg");
                }
            }
            else
            {
                var path = model.Get(url);
                if (path != null)
                {
#if !DEBUG
                    var cache = Response.Cache;
                    cache.SetCacheability(HttpCacheability.Public);
                    cache.SetExpires(new DateTime(2525, 1, 1));
#endif
                    //  return new FilePathResult(path, "image/jpeg");
                    return new FileStreamResult(System.IO.File.Open(path,System.IO.FileMode.Open), "image/jpeg");
                }
            }

            //Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);
        }
    }
}
