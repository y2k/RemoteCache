﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RemoteCache.Services.Downloader;

namespace RemoteCache.Services
{
    class WorkerService : IWorkerService
    {
        readonly WorkerManager workPool;
        readonly ImageStorage storage;
        readonly PreFetcher preFetcher;

        public WorkerService(ImageStorage storage, PreFetcher preFetcher)
        {
            this.preFetcher = preFetcher;
            this.storage = storage;
            this.workPool = new WorkerManager(storage);

            Console.WriteLine("Program start");
            MediaConverter.Instance.ValidateFFMMPEG();

            workPool.Start();
            Console.WriteLine("Initialize downloaders complete");

            preFetcher.Start();
        }

        public string GetPathForImage(Uri url, int width, int height, HttpRequest request)
        {
            preFetcher.RequestImage(url, width, height, request);
            return Validate(url, storage.GetPathForImage(url));
        }

        public Task<string> GetPathForExtraImageAsync(Uri url, string layer)
        {
            return Task.FromResult(Validate(url, storage.GetPathForImage(url, layer)));
        }

        string Validate(Uri source, string file)
        {
            var result = file != null && File.Exists(file) ? file : null;
            if (result == null)
            {
                Console.WriteLine("Get new work " + source);
                workPool.AddWork(source);
            }
            return result;
        }
    }
}