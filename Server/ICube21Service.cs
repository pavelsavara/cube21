using System.Runtime.Serialization;
using System.ServiceModel;
using Zamboch.Cube21;
using Zamboch.Cube21.Actions;

namespace Server
{
    // A WCF service consists of a contract (defined below as ICube21Service).
    [ServiceContract]
    public interface ICube21Service
    {
        [OperationContract]
        Path FindWayHome(Cube cube);
    }
}
