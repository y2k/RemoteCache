using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RemoteCacheService.Models;

namespace RemoteCacheService.Controllers
{
    public class CacheController : Controller
    {
        //        // TODO: убрать костыль когда починят mono asp net
        //        public ActionResult Get()
        //        {
        //            return InnerGet(
        //                Request["url"],
        //                Request["format"],
        //                Request["bgColor"],
        //                Request.AsInt("size"),
        //                Request.AsInt("width"),
        //                Request.AsInt("maxHeight"));
        //        }

        public ActionResult Get(string url, string format, string bgColor, int? size = null, int? width = null, int? maxHeight = null)
        {
            var imageRepository = new RemoteCache();
            if (bgColor != null)
                imageRepository.SetJpegBackground(bgColor);

            Uri tmp;
            if (!Uri.TryCreate(url, UriKind.Absolute, out tmp))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (size.HasValue && (size < 16 || size > 512))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (size.HasValue)
            {
                var data = imageRepository.Square(url, size.Value, format);
                if (data != null)
                {
                    ConfigureCache();
                    return new FileContentResult(data, "image/jpeg");
                }
            }
            else if (width.HasValue && maxHeight.HasValue)
            {
                var data = imageRepository.Thumbnail(url, width.Value, maxHeight.Value, format);
                if (data != null)
                {
                    ConfigureCache();
                    return new FileContentResult(data, "image/jpeg");
                }
            }
            else
            {
                var path = imageRepository.Get(url, format);
                if (path != null)
                {
                    ConfigureCache();
                    return new FilePathResult(path, "mp4" == format ? "video/mp4" : "image/jpeg");
                }
            }

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
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
