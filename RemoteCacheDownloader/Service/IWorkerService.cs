using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCacheDownloader.Service
{
    [ServiceContract]
    public interface IWorkerService
    {
        [OperationContract]
        void AddWork(Uri source);
    }
}