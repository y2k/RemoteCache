using System;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Mvc;
using RemoteCache.Common;
using RemoteCache.Services;

namespace RemoteCache.Controllers
{
    [Route("cache")]
    public class CacheController : Controller
    {
        [FromServices]
        public IWorkerService client { get; set; }

        [FromServices]
        public BaseImageResizer resizer { get; set; }

        [Route("original")]
        public ActionResult Original(string url, string format)
        {
            var path = client.GetPathForExtraImage(new Uri(url), format);
            if (path == null)
            {
                Response.ContentLength = 0;
                Response.Headers["Cache-Control"] = "no-cache";
                return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);
            }

            var m = new Regex(@"/cache/([\w\d/]+\.mp4)").Match(path);
            if (m.Success)
            {
                Response.ContentLength = 0;
                return LocalRedirect("/mp4/" + m.Groups[1].Value);
            }

            Response.Headers["Cache-Control"] = "public, max-age=2419200";
            Response.ContentLength = new System.IO.FileInfo(path).Length;
            return PhysicalFile(path, "mp4" == format ? "video/mp4" : "image/jpeg");
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

            var path = client.GetPathForImage(new Uri(url), width, height);
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