using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.QueueAccess;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface IQueueAccess
    {
        Task<QueueAccessRQResponse> QueueAccessAsync(QueueAccessRQRequest queueAccessRQRequest);
    }
}