using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Microservices.CRM.Email.Domain;
using VAA.EAPI.Microservices.CRM.Email.Mapper;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command
{
    public class SendOrderEmailCommandHandler : ISendOrderEmailCommand
    {
        private readonly IACMClient _aCMClient;
        private readonly ILogger<SendOrderEmailCommandHandler> _logger;

        public SendOrderEmailCommandHandler(IACMClient aCMClient, ILogger<SendOrderEmailCommandHandler> logger)
        {
            _aCMClient = aCMClient ?? throw new ArgumentNullException(nameof(aCMClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(SendOrderEmailRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var acmRequest = request.RequestType == Constants.RequestType.OrderCancel? OrderCancelEmailToACMRequestMapper.MapRequest(request): request.RequestType == Constants.RequestType.HoldBooking? OrderEmailToACMHoldBookingRQMapper.MapRequest(request) : OrderEmailToACMRequestMapper.MapRequest(request);

                await _aCMClient.SendEmail(acmRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendOrderEmailCommandHandler");
                throw;
            }
        }
    }
}