using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RemoteCache.Services.Downloader
{
    class PreFetcher
    {
        private const string UserAgent = "PreFetcher/0.1";
        private const int RestTime = 30 * 1000;
        private const int QueueCapacity = 10000;

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
                await Task.Delay(RestTime);
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
                return new int[] { 27, 54, 81 }
                    .Select(s => new Uri(request.requestUri, "/cache/fit?url=" + dataUri + "&width=" + s + "&height=" + s + "&bgColor=ffffff&quality=30&isNorm=True"));
            }
            if (request.link.AbsolutePath.Contains("/avatar/tag/"))
            {
                return new int[] { 27, 54, 81 }
                    .Select(s => new Uri(request.requestUri, "/cache/fit?url=" + dataUri + "&width=" + s + "&height=" + s + "&bgColor=ffffff&quality=30&isNorm=True"));
            }
            if (request.link.AbsolutePath.Contains("/pics/post/"))
            {
                return new int[] { 162, 243, 486 }
                    .Select(s => new { w = s, h = s * request.height / request.width })
                    .Select(s => new Uri(request.requestUri, "/cache/fit?url=" + dataUri + "&width=" + s.w + "&height=" + s.h + "&bgColor=ffffff&quality=30&isNorm=True"));
            }
            return new Uri[0];
        }

        private async Task DownloadDataTaskAsync(Uri uri)
        {
            // Console.WriteLine("[PREFETCHER]: fetch " + uri);
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                await client.GetByteArrayAsync(uri);
            }
            catch (WebException)
            {
                // Ignore 404
            }
        }

        internal void RequestImage(Uri url, int requestWidth, int requestHeight, HttpRequest request)
        {
            if (request.Headers["User-Agent"] == UserAgent) return;
            lock (queue)
            {
                if (queue.Count < QueueCapacity)
                    queue.Add(new ImageRequest
                    {
                        link = url,
                        width = requestWidth,
                        height = requestHeight,
                        requestUri = new Uri($"{request.Scheme}://{request.Host}"),
                    });
            }
        }

        struct ImageRequest
        {
            internal Uri link;
            internal int width;
            internal int height;
            internal Uri requestUri;
        }
    }
}