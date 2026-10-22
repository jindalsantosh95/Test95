using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class CreatePnrRepoHandler : IRequestHandler<CreatePnrRepoQuery, string>
    {
        private readonly ILogger<CreatePnrRepoHandler> _logger;
        private readonly IBookingProcessV3 _bookingProcess;

        public CreatePnrRepoHandler(ILogger<CreatePnrRepoHandler> logger, IBookingProcessV3 bookingProcess)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bookingProcess = bookingProcess ?? throw new ArgumentNullException(nameof(bookingProcess));
        }

        public async Task<string> Handle(CreatePnrRepoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Booking Process - PerformRevenue delta service call start");
                var response = await _bookingProcess.PerformRevenue(BookingProcessRequestMapper.MapBookingRequest(request._SelectedOffers
                    , request._OriginDestinationAssociations, request._CustomerDetails, request._SegmentDetails, request._PriceClass));

                return response.TeardownContextResponseInfo.Pnr.RecordLocator.First().RecordLocator1;
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception, "Fault Exception from Create Pnr service");
                throw Air4FaultExceptionChecker.PerformRevenue(exception);
            }
            finally
            {
                _logger.LogDebug($"Ticketing Perform Revenue delta service call end");
            }
        }
    }

}
