using System;
using RemoteCacheDownloader.Model;

namespace RemoteCacheDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program start");
            GifConverter.Instance.ValidateFFMMPEG();

            WorkerService.InitializeService();
            Console.WriteLine("Initialize service complete");

            WorkerManager.Instance.Start();
            Console.WriteLine("Initialize downloaders complete");

            Console.ReadLine();
        }
    }
}