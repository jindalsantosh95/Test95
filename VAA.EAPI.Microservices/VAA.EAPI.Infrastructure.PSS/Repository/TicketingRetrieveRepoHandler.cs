using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class TicketingRetrieveRepoHandler : IRequestHandler<TicketingRetrieveRepoQuery, RetrieveResponseType>
    {
        private readonly ILogger<TicketingRetrieveRepoHandler> _logger;
        private readonly ITicketingProcessV3 _ticketingProcessV3;

        public TicketingRetrieveRepoHandler(ILogger<TicketingRetrieveRepoHandler> logger, ITicketingProcessV3 ticketingProcessV3)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ticketingProcessV3 = ticketingProcessV3 ?? throw new ArgumentNullException(nameof(ticketingProcessV3));
        }

        public async Task<RetrieveResponseType> Handle(TicketingRetrieveRepoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Ticketing Retrieve delta service call start for ticket - {request.RetrieveRequestType.TicketRequest.First().ElectronicDocumentNumber}");
                var response = await Task.FromResult(_ticketingProcessV3.Retrieve(request.RetrieveRequestType));

                if (response.TicketRequestResponse.Any(x => x.Error != null))
                {
                    throw new ExternalServiceIntegrationException(string.Empty, new Exception($"{Constants.Application.ErrorCode} : {response.TicketRequestResponse.First(x => x.Error != null).Error.ErrorCode}  {Constants.Application.ErrorMessage} : {response.TicketRequestResponse.First(x => x.Error != null).Error.ErrorText}"));
                }

                return response;
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception, "Fault exception from Ticketing retrieve service");
                throw Air4FaultExceptionChecker.TicketingRetrieveException(exception);
            }
            finally
            {
                _logger.LogInformation($"Ticketing Retrieve delta service call end for ticket - {request.RetrieveRequestType.TicketRequest.First().ElectronicDocumentNumber}");
            }
        }
    }

}
