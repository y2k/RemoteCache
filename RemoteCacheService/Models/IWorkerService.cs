using System;
using System.ServiceModel;

namespace RemoteCacheDownloader.Service
{
    [ServiceContract]
    public interface IWorkerService
    {
        [OperationContract(Action="AddWork")]
        void AddWork(Uri source);

        [OperationContract(Action="GetPathForImage")]
        string GetPathForImage(Uri url);
    }
}