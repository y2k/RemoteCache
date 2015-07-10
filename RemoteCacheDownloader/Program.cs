using System;
using RemoteCacheDownloader.Model;

namespace RemoteCacheDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program start");

            DownloadWorker.GetFFFMPEG();

            WorkerService.InitializeService();
            Console.WriteLine("Initialize service complete");

            WorkerManager.Instance.Start();
            Console.WriteLine("Initialize downloaders complete");

            Console.ReadLine();
        }
    }
}