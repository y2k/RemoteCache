using System;
using RemoteCache.Worker.Model;
using RemoteCache.Worker.Service;

namespace RemoteCache.Worker
{
	public class Program
	{
		public static void Main() 
		{
            Console.WriteLine("Program start");
            GifConverter.Instance.ValidateFFMMPEG();

            WorkerService.InitializeService();
            Console.WriteLine("Initialize service complete");

            WorkerManager.Instance.Start();
            Console.WriteLine("Initialize downloaders complete");
		}
	}
}