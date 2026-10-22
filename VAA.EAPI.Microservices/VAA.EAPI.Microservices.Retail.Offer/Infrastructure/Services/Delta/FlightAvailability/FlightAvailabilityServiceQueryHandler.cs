using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Models;
using VAA.EAPI.Infrastructure.PSS.Repository.Command;
using VAA.EAPI.Infrastructure.PSS.Repository.Query;
using VAA.EAPI.Microservices.Retail.Offer.Constants;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightAvailability
{
    public class FlightAvailabilityServiceQueryHandler : IRequestHandler<FlightAvailabilityServiceQuery, Segment>
    {
        public readonly ILogger<FlightAvailabilityServiceQueryHandler> _logger;
        private readonly IMediator _mediator;

        public FlightAvailabilityServiceQueryHandler(ILogger<FlightAvailabilityServiceQueryHandler> logger, IMediator mediatR)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _mediator = mediatR ?? throw new System.ArgumentNullException(nameof(mediatR));
        }

        public async Task<Segment> Handle(FlightAvailabilityServiceQuery request, CancellationToken cancellationToken)
        {
            Air4RequestContext air4Context = new()
            {
                AppChannelName = ConfigReader.Get(Environment.AppChannelName),
                AppId = ConfigReader.Get(Environment.Air4ItliteApplicationId),
                DebugInfo = ConfigReader.Get(Environment.DebugOptions),
                PoolType = ConfigReader.Get(Environment.Air4ItlitePoolType),
                TestLab = ConfigReader.Get(Environment.TestLab),
                TimeoutInactivity = ConfigReader.Get(Environment.HostContextTimeoutInactivity),
                TimeoutSession = ConfigReader.Get(Environment.HostContextTimeoutSession),
            };


            CreateAndSwingCityCommand contextCommand = new()
            {
                PointOfSale = request.PointOfSale,
                Air4RequestContext = air4Context
            };
            var sessionId = await _mediator.Send(contextCommand, cancellationToken);

            FlightSchedulesAvailabilityQuery availabilityQuery = new()
            {
                FlightDate = request.FlightDate,
                Carrier = request.Carrier,
                FlightNumber = request.FlightNumber.ToString("0000"),
                Origin = request.Origin,
                Destination = request.Destination,
                SessionId = sessionId,
                Air4RequestContext = air4Context
            };
            return await _mediator.Send(availabilityQuery, cancellationToken);
        }
    }
}
