using System;
using System.ServiceModel;

namespace RemoteCacheApi.Models
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