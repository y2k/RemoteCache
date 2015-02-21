using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RemoteCacheService.Models;

namespace RemoteCacheService.Controllers
{
    public class CacheController : Controller
    {
        CacheModel model = new CacheModel();

        // TODO: убрать костыль когда починят mono asp net
        public ActionResult Get() {
            return InnerGet(
                Request["url"],
                Request["format"],
                Request.AsInt("size"),
                Request.AsInt("width"),
                Request.AsInt("maxHeight"));
        }

        ActionResult InnerGet(string url, string format, int? size = null, int? width = null, int? maxHeight = null)
        {
            Uri tmp;
            if (!Uri.TryCreate(url, UriKind.Absolute, out tmp))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (size.HasValue && (size < 16 || size > 512))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

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
                    return new FileContentResult(data, "image/jpeg");
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
                    return new FilePathResult(path, "image/jpeg");
                }
            }

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }
    }
}
