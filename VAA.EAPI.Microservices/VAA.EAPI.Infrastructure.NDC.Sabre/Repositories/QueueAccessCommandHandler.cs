using System;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.QueueAccess;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class QueueAccessCommandHandler(IQueueAccess queueAccess) : IQueueAccessCommand
    {
        private readonly IQueueAccess _queueAccess = queueAccess ?? throw new ArgumentNullException(nameof(queueAccess));

        public async Task<QueueAccessRQResponse> QueueAccess(QueueAccessRQRequest queueAccessRQRequest)
        {
            if (queueAccessRQRequest == null) throw new ArgumentNullException(nameof(queueAccessRQRequest));

            return await _queueAccess.QueueAccessAsync(queueAccessRQRequest);
        }
    }
}