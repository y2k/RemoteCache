using System;
using System.ServiceModel;

namespace RemoteCacheDownloader.Service
{
    [ServiceContract]
    public interface IWorkerService
    {
        [OperationContract]
        void AddWork(Uri source);

        [OperationContract]
        string GetPathForImage(Uri source);
    }
}