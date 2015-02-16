using NLog;
using RemoteCacheDownloader.Model;
using RemoteCacheDownloader.Service;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace RemoteCacheDownloader
{
    public class WorkerService : IWorkerService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #region IWorkerService Members

        public void AddWork(Uri source)
        {
            Log.Trace("Get new work {0}" + source);
            WorkerManager.Instance.AddWork(source);
        }

        public string GetPathForImage(Uri source)
        {
            return WorkerManager.Instance.GetPathForImage(source);
        }

        #endregion

        public static void InitializeService()
        {
            Uri baseAddress = new Uri("http://localhost:8191/remote-cache");
            ServiceHost host = new ServiceHost(typeof(WorkerService), baseAddress);
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            host.Description.Behaviors.Add(smb);

            host.Open();
        }
    }
}