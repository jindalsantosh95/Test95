using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Email.Entities;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Email.Mapper;

namespace VAA.EAPI.Infrastructure.Email.Repository
{
    public class CommunicationRepository : ICommunicationRepository
    {
        private readonly IWorkflowClient _IWorkflowClient;
        private readonly ILogger<CommunicationRepository> _logger;

        public CommunicationRepository(ILogger<CommunicationRepository> logger, IWorkflowClient IWorkflowClient)
        {
            _IWorkflowClient = IWorkflowClient ?? throw new ArgumentNullException(nameof(IWorkflowClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Notify(SendEmailRequest request, string CorrelationId, CancellationToken cancellationToken)
        {
            var workFlowRequest = WorkFlowRequestMapper.MapRequest(request);
            _logger.LogInformation($"Process started to Notify| for correlationId : {CorrelationId}");

            var result = await _IWorkflowClient.SendEmailCommunication(workFlowRequest, cancellationToken);
            var workFlowResponse = Json.Deserialize<WorkFlowResponse>(result.Value);

            if (result.Key)
            {
                _logger.LogInformation($"WorkFlow Response for correlationId:{CorrelationId} | Key : {result.Key} | Value : {result.Value}");
                return workFlowResponse.Success;
            }
            return false;
        }
    }
}
