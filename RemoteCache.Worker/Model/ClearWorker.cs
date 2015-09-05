using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace RemoteCache.Worker.Model
{
    class ClearWorker
    {
        static readonly TimeSpan SleepTime = new TimeSpan(0, 20, 0); // 20 minutes

        ImageStorage cacheRoot;
        long maxCacheSize;
        const float trimFactor = 0.8f; // Коэф. размера кэша до которого он уменьшается при привышение лимита.

        public ClearWorker(ImageStorage cacheRoot, long maxCacheSize)
        {
            this.cacheRoot = cacheRoot;
            this.maxCacheSize = maxCacheSize;
        }

        internal void Start()
        {
            new Thread(() =>
            {
                try
                {
                    Execute();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Thread.Sleep(SleepTime);
            }).Start();
        }

        private void Execute()
        {
            Console.WriteLine("Start clear");
            var files = Directory.EnumerateFiles(cacheRoot.GetRootDirectory())
                .Where(s => !s.EndsWith("*.tmp"))
                .Select(s => new FileInfo(s))
                .OrderByDescending(s => s.LastWriteTime)
                .ToList();
            Console.WriteLine("Get all files, count = {0}", files.Count);

            if (files.Sum(s => s.Length) <= maxCacheSize) return; // Если кэш меньши лимита, то выходим

            long total = 0;
            foreach (var f in files)
            {
                total += f.Length;
                if (total > (long)(maxCacheSize * trimFactor))
                {
                    try
                    {
                        f.Delete();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error while delete file " + f.FullName + "\n" + e);
                    }
                }
            }

            Console.WriteLine("End delete file");
        }
    }
}