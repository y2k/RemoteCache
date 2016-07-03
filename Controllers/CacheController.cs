using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RemoteCache.Common;
using RemoteCache.Services;

namespace RemoteCache.Controllers
{
    [Route("cache")]
    public class CacheController : Controller
    {
        readonly IWorkerService client;
        readonly BaseImageResizer resizer;

        public CacheController([FromServices] IWorkerService client, [FromServices] BaseImageResizer resizer)
        {
            this.client = client;
            this.resizer = resizer;
        }

        [Route("original")]
        public async Task<ActionResult> Original(string url, string format)
        {
            var path = await client.GetPathForExtraImage(new Uri(url), format);
            if (path == null)
            {
                Response.ContentLength = 0;
                Response.Headers["Cache-Control"] = "no-cache";
                return new StatusCodeResult((int)HttpStatusCode.NotFound);
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
        public async Task<ActionResult> Fit(string url, int width, int height, string bgColor, int? quality)
        {
            var normWidth = new WithAnalyzer(width, height);
            if (!normWidth.IsNormalized)
            {
                Console.WriteLine("REDIRECT [{0}x{1}] -> [{2}x{3}] | ({5}q) {4}", width, height, normWidth.NormWidth, normWidth.NormHeight, url, quality);
                Response.ContentLength = 0;
                return RedirectToAction("Fit", new { url, width = normWidth.NormWidth, height = normWidth.NormHeight, bgColor, quality, isNorm = true });
            }
            else
            {
                Console.WriteLine("NO redirect [{0}x{1}] | ({3}q) {2}", width, height, url, quality);
            }

            var path = await client.GetPathForImage(new Uri(url), width, height, this.Request);
            if (path == null)
            {
                Response.ContentLength = 0;
                return new StatusCodeResult((int)HttpStatusCode.NotFound);
            }

            resizer.BackgroundColor = bgColor;

            var result = await resizer.GetRectAsync(quality, path, width, height);
            Response.Headers["Cache-Control"] = "public, max-age=2419200";
            Response.ContentLength = result.Length;
            return File(result, "image/jpeg");
        }
    }
}