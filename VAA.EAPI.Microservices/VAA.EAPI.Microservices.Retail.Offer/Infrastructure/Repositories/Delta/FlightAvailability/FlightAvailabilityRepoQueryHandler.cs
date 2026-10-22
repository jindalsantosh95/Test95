using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightAvailability;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Delta.FlightAvailability
{
    public class FlightAvailabilityRepoQueryHandler : IRequestHandler<FlightAvailabilityRepoQuery, FlightAvailabilityResponse>
    {
        public readonly ILogger<FlightAvailabilityRepoQueryHandler> Logger;
        public readonly IMediator Mediator;

        public FlightAvailabilityRepoQueryHandler(ILogger<FlightAvailabilityRepoQueryHandler> logger, IMediator mediator)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<FlightAvailabilityResponse> Handle(FlightAvailabilityRepoQuery request, CancellationToken cancellationToken)
        {
            Segment seg = await Mediator.Send(new FlightAvailabilityServiceQuery(request));
            return new()
            {
                Availability = seg,
                Generated = DateTimeOffset.UtcNow
            };
        }
    }
}
