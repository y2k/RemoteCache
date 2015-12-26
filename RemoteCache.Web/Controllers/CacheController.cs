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
        BaseImageResizer resizer = new BitMiracleImageResizer();

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
        public ActionResult Fit(string url, int width, int height, string bgColor, int? quality)
        {
            var path = imageRepository.Get(url, null);
            if (path == null) {
                Response.ContentLength = 0;
                return new HttpStatusCodeResult((int)HttpStatusCode.NotFound);
            }

            resizer.BackgroundColor = bgColor;

            var result = resizer.GetRect(quality, path, width, height);
            Response.ContentLength = result.Length;
            return File(result, "image/jpeg");
        }
    }
}