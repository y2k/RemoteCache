using System;
using System.IO;
using System.Net;
using Microsoft.AspNet.Mvc;
using RemoteCache.Common;
using RemoteCache.Web.Models;

namespace RemoteCache.Web.Controllers
{
    [Route("[controller]")]
    public class CacheController : Controller
    {
        [FromServices]
        public RemoteImageRepository imageRepository { get; set; }

        [FromServices]
        public BaseImageResizer resizer { get; set; }

        [Route("original")]
        public ActionResult Original(string url, string format)
        {
            var path = imageRepository.Get(url, format);
            if (path == null)
            {
                Response.ContentLength = 0;
                return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);
            }

            var data = new FileStream(path, FileMode.Open);
            Response.Headers["Cache-Control"] = "public, max-age=2419200";
            Response.ContentLength = data.Length;
            return File(data, "mp4" == format ? "video/mp4" : "image/jpeg");
        }

        [Route("fit")]
        public ActionResult Fit(string url, int width, int height, string bgColor, int? quality)
        {
            var normWidth = new WithAnalyzer(width, height);
            if (!normWidth.IsNormalized)
            {
                // Console.WriteLine("REDIRECT [{0}x{1}] -> [{2}x{3}] | ({5}q) {4}", width, height, normWidth.NormWidth, normWidth.NormHeight, url, quality);
                Response.ContentLength = 0;
                return RedirectToAction("Fit", new { url, width = normWidth.NormWidth, height = normWidth.NormHeight, bgColor, quality, isNorm = true });
            }
            // else
            // {
            //     Console.WriteLine("NO redirect [{0}x{1}] | ({3}q) {2}", width, height, url, quality);
            // }

            var path = imageRepository.Get(url, width, height);
            if (path == null)
            {
                Response.ContentLength = 0;
                return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);
            }

            resizer.BackgroundColor = bgColor;

            var result = resizer.GetRect(quality, path, width, height);
            Response.Headers["Cache-Control"] = "public, max-age=2419200";
            Response.ContentLength = result.Length;
            return File(result, "image/jpeg");
        }
    }
}