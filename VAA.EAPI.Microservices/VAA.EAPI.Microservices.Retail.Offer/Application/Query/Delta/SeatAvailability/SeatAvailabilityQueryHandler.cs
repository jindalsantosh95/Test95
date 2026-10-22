using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.SeatAvailability
{
    public class SeatAvailabilityQueryHandler : IRequestHandler<SeatAvailabilityQuery, Result<Response<SeatAvailabilityResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SeatAvailabilityQueryHandler> _logger;

        public SeatAvailabilityQueryHandler(IMediator mediator, ILogger<SeatAvailabilityQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<SeatAvailabilityResponse>>> Handle(SeatAvailabilityQuery requestData, CancellationToken cancellationToken)
        {
            SeatAvailabilityResponse seatAvailabilityResponse = new SeatAvailabilityResponse();
            return new Response<SeatAvailabilityResponse>
            {
                Status = Status.OK,
                Value = seatAvailabilityResponse
            };
        }

    }
}
