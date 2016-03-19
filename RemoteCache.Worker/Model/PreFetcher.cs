using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RemoteCache.Worker.Model
{
    class PreFetcher
    {
        Uri baseUri = new Uri("https://api-i-twister.net:8011/");
        // Uri baseUri = new Uri("http://localhost:8080/");
        HashSet<ImageRequest> queue = new HashSet<ImageRequest>();

        internal async void Start()
        {
            while (true)
            {
                foreach (var uri in GetLocalCopy().SelectMany(s => MakeSubImageUris(s)))
                {
                    await DownloadDataTaskAsync(uri);
                    await Task.Delay(1000);
                }
                await Task.Delay(30 * 1000);
            }
        }

        List<ImageRequest> GetLocalCopy()
        {
            lock (queue)
            {
                var localQueue = queue.ToList();
                queue.Clear();
                return localQueue;
            }
        }

        IEnumerable<Uri> MakeSubImageUris(ImageRequest request)
        {
            var dataUri = Uri.EscapeDataString(request.link.AbsoluteUri);
            if (request.link.AbsolutePath.Contains("/avatar/user/"))
            {
                return new int[] { 32, 48, 64 }
                    .Select(s => new Uri(baseUri, "/Cache/fit?url=" + dataUri + "&width=" + s + "&height=" + s + "&bgColor=ffffff&quality=30&isNorm=True"));
            }
            if (request.link.AbsolutePath.Contains("/avatar/tag/"))
            {
                return new int[] { 32, 48, 64 }
                    .Select(s => new Uri(baseUri, "/Cache/fit?url=" + dataUri + "&width=" + s + "&height=" + s + "&bgColor=ffffff&quality=30&isNorm=True"));
            }
            if (request.link.AbsolutePath.Contains("/pics/post/"))
            {
                return new int[] { 192, 256, 512 }
                    .Select(s => new { w = s, h = s * request.height / request.width })
                    .Select(s => new Uri(baseUri, "/Cache/fit?url=" + dataUri + "&width=" + s.w + "&height=" + s.h + "&bgColor=ffffff&quality=30&isNorm=True"));
            }
            return new Uri[0];
        }

        private async Task DownloadDataTaskAsync(Uri uri)
        {
            // Console.WriteLine("[PREFETCHER]: fetch " + uri);
            try
            {
                var client = new WebClient();
                client.Headers["User-Agent"] = "PreFetcher/0.1";
                await client.DownloadDataTaskAsync(uri);
            }
            catch (WebException)
            {
                // Ignore 404
            }
        }

        internal void RequestImage(Uri url, int requestWidth, int requestHeight)
        {
            lock (queue)
            {
                if (queue.Count < 10000)
                    queue.Add(new ImageRequest { link = url, width = requestWidth, height = requestHeight });
            }
        }

        struct ImageRequest
        {
            internal Uri link;
            internal int width;
            internal int height;
        }
    }
}