using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Models;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class TicketingV3RetrieveRepoQueryHandler : IRequestHandler<TicketingV3RetrieveRepoQuery, TicketingV3RetrieveResponse>
    {
        private readonly ILogger<TicketingV3RetrieveRepoQueryHandler> _logger;
        private readonly ITicketingProcessV3 _ticketingProcessV3;

        public TicketingV3RetrieveRepoQueryHandler(ILogger<TicketingV3RetrieveRepoQueryHandler> logger, ITicketingProcessV3 ticketingProcessV3)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ticketingProcessV3 = ticketingProcessV3 ?? throw new ArgumentNullException(nameof(ticketingProcessV3));
        }
        public Task<TicketingV3RetrieveResponse> Handle(TicketingV3RetrieveRepoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Ticketing Retrieve delta service call start");
                var response = _ticketingProcessV3.Retrieve(request.RetrieveRequestType);
                return Task.FromResult(TicketingV3RetrieveRSMapper.MapToTicketingV3RetrieveResponse(response));
            }
            catch (FaultException ex)
            {
                _logger.LogInformation($"Ticketing Retrieve delta service error");
                throw Air4FaultExceptionChecker.TicketingRetrieveException(ex);
            }
            finally
            {
                _logger.LogInformation($"Ticketing Retrieve delta service call end");
            }
        }
    }
}
