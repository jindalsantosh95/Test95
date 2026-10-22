using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class TicketingEmailReceiptQueryHandler : IRequestHandler<TicketingEmailReceiptQuery, EmailReceiptResponse>
    {
        private readonly ILogger<TicketingEmailReceiptQueryHandler> _logger;
        private readonly ITicketingProcessV3 _ticketingProcessV3;

        public TicketingEmailReceiptQueryHandler(ILogger<TicketingEmailReceiptQueryHandler> logger, ITicketingProcessV3 ticketingProcessV3)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ticketingProcessV3 = ticketingProcessV3 ?? throw new ArgumentNullException(nameof(ticketingProcessV3));
        }

        public async Task<EmailReceiptResponse> Handle(TicketingEmailReceiptQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _ticketingProcessV3.EmailReceipt(TicketingV3EmailReceiptRQMapper.MapToEmailReceiptRequest(request.SessionId, request.EmailId, false));

                if (response.EmailReceiptResponse1.Error != null)
                {
                    throw new ExternalServiceIntegrationException(string.Empty, new Exception($"{Constants.Application.ErrorCode} : {response.EmailReceiptResponse1.Error.ErrorCode}  {Constants.Application.ErrorMessage} : {response.EmailReceiptResponse1.Error.ErrorText}"));
                }

                return response;
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception, "Fault exception from Ticketing email receipt service");
                throw Air4FaultExceptionChecker.TicketingProcessV3Exception(exception);
            }
        }
    }
}
