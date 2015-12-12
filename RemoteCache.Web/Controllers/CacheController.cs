using System;
using System.IO;
using System.Net;
using Microsoft.AspNet.Mvc;
using RemoteCache.Web.Models;

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
            if (path == null) {
                Response.ContentLength = 0;
                return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);
            }

            var data = new FileStream(path, FileMode.Open);
            Response.ContentLength = data.Length;
            return File(data, "mp4" == format ? "video/mp4" : "image/jpeg");
        }

        [Route("fit")]
        public ActionResult Fit(string url, int width, int height, string bgColor)
        {
            var path = imageRepository.Get(url, null);
            if (path == null) {
                Response.ContentLength = 0;
                return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);
            }

            if (bgColor != null)
                resizer.SetJpegBackground(bgColor);

            var aspect = (float)width / height;
            var result = resizer.GetRect(path, width, aspect, aspect);
            Response.ContentLength = result.Length;
            return File(result, "image/jpeg");
        }

        [Route("fitWidth")]
        [Obsolete]
        public ActionResult FitWidth(string url, int width, string bgColor, float? minAspect = null, float? maxAspect = null)
        {
            var path = imageRepository.Get(url, null);
            if (path == null) {
                Response.ContentLength = 0;
                return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);
            }

            if (bgColor != null)
                resizer.SetJpegBackground(bgColor);

            var result = resizer.GetRect(path, width, minAspect ?? 0.5f, maxAspect ?? 2);
            Response.ContentLength = result.Length;
            return File(result, "image/jpeg");
        }

        [Route("fitSize")]
        [Obsolete]
        public ActionResult FitSize(string url, int size, string format, string bgColor)
        {
            var path = imageRepository.Get(url, null);
            if (path == null) {
                Response.ContentLength = 0;
                return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);
            }

            if (bgColor != null)
                resizer.SetJpegBackground(bgColor);

            var result = resizer.GetRect(path, size);
            Response.ContentLength = result.Length;
            return File(result, "image/jpeg");
        }
    }
}