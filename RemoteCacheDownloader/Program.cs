using NLog;
using RemoteCacheDownloader.Model;
using System;
using System.Collections.Generic;

namespace RemoteCacheDownloader
{
    class Program
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly TimeSpan WaitPeriod = new TimeSpan(0, 0, 1);

        private const int ImagePerChunk = 100;
        private const int MaxThreads = 20;

        private static readonly HashSet<string> LockedUrls = new HashSet<string>();

        static void Main(string[] args)
        {
            Log.Debug("Program start");

            WorkerService.InitializeService();
            Log.Debug("Initialize service complete");

            WorkerManager.Instance.Start();
            Log.Debug("Initialize downloaders complete");

            Console.ReadLine();
        }
    }
}