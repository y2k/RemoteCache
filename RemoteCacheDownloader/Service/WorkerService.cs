using RemoteCacheDownloader.Model;
using RemoteCacheDownloader.Service;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace RemoteCacheDownloader
{
    public class WorkerService : IWorkerService
    {
        #region IWorkerService Members

        public void AddWork(Uri source)
        {
            Console.WriteLine("Get new work {0}" + source);
            WorkerManager.Instance.AddWork(source);
        }

        public string GetPathForImage(Uri url)
        {
            var file = new ImageStorage().GetPathForImage(url);
            return file != null && File.Exists(file) ? file : null;
//            if (file == null || File.Exists(file))
//                return file;
//            throw new Exception();
        }

        #endregion

        //        public static void InitializeService()
        //        {
        //            var baseAddress = new Uri("http://localhost:8191/remote-cache");
        //            var host = new ServiceHost(typeof(WorkerService), baseAddress);
        //            var smb = new ServiceMetadataBehavior();
        //            smb.HttpGetEnabled = true;
        //            //smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
        //            host.Description.Behaviors.Add(smb);
        //
        //            host.Open();
        //        }

        public static void InitializeService()
        {
            var host = new ServiceHost(typeof(WorkerService), new Uri("http://localhost:8192/remote-cache"));
//            var host = new ServiceHost(typeof(WorkerService));
//            host.AddServiceEndpoint(typeof(IWorkerService), new NetTcpBinding(), new Uri("net.tcp://localhost:8192/remote-cache"));
            host.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = false });
            host.Open();
        }
    }
}