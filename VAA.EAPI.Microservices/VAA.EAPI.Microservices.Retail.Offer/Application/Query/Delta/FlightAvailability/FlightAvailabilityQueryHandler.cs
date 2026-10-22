using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Delta.FlightAvailability;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightAvailability
{
    public class FlightAvailabilityQueryHandler : IRequestHandler<FlightAvailabilityQuery, Result<Response<FlightAvailabilityResponse>>>
    {
        public readonly IMediator _mediator;
        public readonly ILogger<FlightAvailabilityQueryHandler> _logger;

        public FlightAvailabilityQueryHandler(IMediator mediator, ILogger<FlightAvailabilityQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<FlightAvailabilityResponse>>> Handle(FlightAvailabilityQuery request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new FlightAvailabilityRepoQuery(request), cancellationToken);
            response.Request = request;

            Result<Response<FlightAvailabilityResponse>> commandOutput = new Response<FlightAvailabilityResponse>
            {
                Status = Status.OK,
                Value = response
            };

            return commandOutput;
        }
    }
}
