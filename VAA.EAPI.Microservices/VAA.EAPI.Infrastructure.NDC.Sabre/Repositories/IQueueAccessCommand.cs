using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.QueueAccess;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public interface IQueueAccessCommand
    {
        Task<QueueAccessRQResponse> QueueAccess(QueueAccessRQRequest queueAccessRQRequest);
    }
}