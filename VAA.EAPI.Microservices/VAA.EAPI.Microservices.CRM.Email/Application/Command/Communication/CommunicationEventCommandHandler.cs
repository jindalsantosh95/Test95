using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Microservices.CRM.Email.Domain;
using VAA.EAPI.Microservices.CRM.Email.Mapper;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command.Communication
{
    public class CommunicationEventCommandHandler : IRequestHandler<CommunicationEventCommand, bool>
    {
        private readonly ILogger<CommunicationEventCommandHandler> _logger;
        private readonly ICommunicationRepository _communicationRepository;

        public CommunicationEventCommandHandler(ILogger<CommunicationEventCommandHandler> logger, ICommunicationRepository communicationRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _communicationRepository = communicationRepository ?? throw new ArgumentNullException(nameof(communicationRepository));
        }

        public async Task<bool> Handle(CommunicationEventCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"CommunicationEventCommand Event Subscribe Handler Start for CorrelationId: {request.Id}");
            try
            {
                var refundFailureRequest = Json.Deserialize<RefundFailureRequest>(Convert.ToString(request.Data));

                var communicationNotifyRequest = CommunicationMapper.RefundFailureRequestMapper(refundFailureRequest);

                if (await _communicationRepository.Notify(communicationNotifyRequest, request.Id, cancellationToken))
                {
                    _logger.LogDebug("Communication is sent successfully");
                    return true;
                }
                else
                {
                    _logger.LogError("Communication is failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sending Communication is failed - {ex.Message} for CorrelationId: {request.Id}");
                throw;
            }
        }
    }
}