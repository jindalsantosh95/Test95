using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.QueueAccess;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class QueueAccess(QueueAccessPortTypeClient queueAccessPortTypeClient, ILogger<QueueAccess> logger) : IQueueAccess
    {
        private readonly QueueAccessPortTypeClient _queueAccessPortTypeClient = queueAccessPortTypeClient ?? throw new ArgumentNullException(nameof(queueAccessPortTypeClient));
        private readonly ILogger<QueueAccess> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<QueueAccessRQResponse> QueueAccessAsync(QueueAccessRQRequest queueAccessRQRequest)
        {
            try
            {
                var response = await _queueAccessPortTypeClient.QueueAccessRQAsync(queueAccessRQRequest.MessageHeader, queueAccessRQRequest.Security, queueAccessRQRequest.QueueAccessRQ);

                return response;
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, $"Error while accessing the queue due to : {ex.Message}");
                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}