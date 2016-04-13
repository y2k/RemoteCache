using System;
using System.IO;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RemoteCache.Services;
using RemoteCache.Services.Downloader;

namespace RemoteCache
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add MVC services to the services container.
            services.AddMvc();
            
            services.AddSingleton<IWorkerService, WorkerService>();
            services.AddTransient<BaseImageResizer, LibGDResizer>();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            // Add the following to the request pipeline only in development environment.
            if (string.Equals(env.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
            {
                app.UseDeveloperExceptionPage();
            }

            // Add static files to the request pipeline.
            Directory.CreateDirectory(ImageStorage.CacheRoot);
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(ImageStorage.CacheRoot),
                RequestPath = new PathString("/mp4"),
            });

            // Add MVC to the request pipeline.
            app.UseMvcWithDefaultRoute();
        }
    }
}