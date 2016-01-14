using System;
using System.ServiceModel;

namespace RemoteCache.Worker.Service
{
    [ServiceContract]
    public interface IWorkerService
    {
        [OperationContract(Action = "AddWork")]
        void AddWork(Uri source);

        [OperationContract(Action = "GetPathForImage")]
        string GetPathForImage(Uri url, int preferedWidth, int preferedHeight);

        [OperationContract(Action = "GetPathForExtraImage")]
        string GetPathForExtraImage(Uri url, string format);
    }
}