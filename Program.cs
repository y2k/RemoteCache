using System.IO;
using Microsoft.AspNetCore.Hosting;
using RemoteCache.Services.Downloader;

namespace RemoteCache
{
    public class Program
    {
        public static void Main()
        {
            MediaConverter.Instance.ValidateFFMMPEG();

            new WebHostBuilder()
                .UseUrls("http://0.0.0.0:5000/")
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build().Run();
        }
    }
}