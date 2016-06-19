using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace RemoteCache
{
    public class Program
    {
        public static void Main()
        {
            new WebHostBuilder()
                .UseUrls("http://:5000/")
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build().Run();
        }
    }
}