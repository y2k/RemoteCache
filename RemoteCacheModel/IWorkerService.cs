using System;
using System.ServiceModel;

namespace RemoteCacheModel
{
    [ServiceContract]
    public interface IWorkerService
    {
        [OperationContract(Action = "AddWork")]
        void AddWork(Uri source);

        [OperationContract(Action = "GetPathForImage")]
        string GetPathForImage(Uri url);

        [OperationContract(Action = "GetPathForExtraImage")]
        string GetPathForExtraImage(Uri url, string format);
    }
}