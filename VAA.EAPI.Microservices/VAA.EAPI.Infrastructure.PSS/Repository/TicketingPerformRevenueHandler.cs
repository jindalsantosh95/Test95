using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class TicketingPerformRevenueHandler : IRequestHandler<TicketingPerformRevenueQuery, BookingDetails>
    {
        private readonly ILogger<TicketingPerformRevenueHandler> _logger;
        private readonly ITicketingProcessV3 _ticketingProcessV3;

        public TicketingPerformRevenueHandler(ILogger<TicketingPerformRevenueHandler> logger, ITicketingProcessV3 ticketingProcessV3)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ticketingProcessV3 = ticketingProcessV3 ?? throw new ArgumentNullException(nameof(ticketingProcessV3));
        }

        public async Task<BookingDetails> Handle(TicketingPerformRevenueQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug($"Ticketing Perform Revenue delta service call start");
                var response = await _ticketingProcessV3.PerformRevenue(TicketingProcessPerformRevenueMapper.RequestMapper(request._SessionId, request._PaymentsDetails, request._Customer));
                return TicketingProcessPerformRevenueMapper.ResponseMapper(response.PerformRevenueResponse1.Pnr);
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception, "Fault Exception from Ticketing  service");
                throw Air4FaultExceptionChecker.TicketingRetrieveException(exception);
            }
            finally
            {
                _logger.LogDebug($"Ticketing Perform Revenue delta service call end");
            }

        }
    }
}
