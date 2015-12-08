using Microsoft.AspNet.Mvc;
using RemoteCache.Web.Models;
using System.IO;
using System.Net;

namespace RemoteCache.Web.Controllers
{
    [Route("[controller]")]
    public class CacheController : Controller
    {
        RemoteImageRepository imageRepository = new RemoteImageRepository();
        BaseImageResizer resizer = new DefaultImageResizer();

        [Route("original")]
        public ActionResult Original(string url, string format)
        {
            var path = imageRepository.Get(url, format);
            if (path == null)
                return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);

            ConfigureCache();
            var data = new FileStream(path, FileMode.Open);
            Response.ContentLength = data.Length;
            return File(data, "mp4" == format ? "video/mp4" : "image/jpeg");
        }

        [Route("fit")]
        public ActionResult Fit(string url, int width, int height, string bgColor)
        {
            var path = imageRepository.Get(url, null);
            if (path == null)
                return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);

            if (bgColor != null)
                resizer.SetJpegBackground(bgColor);

            var aspect = (float)width / height;
            var result = resizer.GetRect(path, width, aspect, aspect);
            ConfigureCache();
            Response.ContentLength = result.Length;
            return File(result, "image/jpeg");
        }

        [Route("fitWidth")]
        public ActionResult FitWidth(string url, int width, string bgColor, float? minAspect = null, float? maxAspect = null)
        {
            var path = imageRepository.Get(url, null);
            if (path == null)
                return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);

            if (bgColor != null)
                resizer.SetJpegBackground(bgColor);

            var result = resizer.GetRect(path, width, minAspect ?? 0.5f, maxAspect ?? 2);
            ConfigureCache();
            Response.ContentLength = result.Length;
            return File(result, "image/jpeg");
        }

        [Route("fitSize")]
        public ActionResult FitSize(string url, int size, string format, string bgColor)
        {
            var path = imageRepository.Get(url, null);
            if (path == null)
                return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);

            if (bgColor != null)
                resizer.SetJpegBackground(bgColor);

            var result = resizer.GetRect(path, size);
            ConfigureCache();
            Response.ContentLength = result.Length;
            return File(result, "image/jpeg");
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