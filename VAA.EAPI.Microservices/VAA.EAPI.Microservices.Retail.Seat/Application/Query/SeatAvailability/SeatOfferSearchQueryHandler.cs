using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Seat.Constants;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Application.Query.SeatAvailability
{

    public class SeatOfferSearchQueryHandler : IRequestHandler<SeatOfferSearchQuery, Result<Response<SeatAvailabilityResponse>>>
    {
        private readonly IMediator _mediator;

        public SeatOfferSearchQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<SeatAvailabilityResponse>>> Handle(SeatOfferSearchQuery request, CancellationToken cancellationToken)
        {
            var seatOfferResponse = await _mediator.Send(new SeatOfferSearchRepoQuery(request.SeatOfferID), cancellationToken);

            if (seatOfferResponse.FlightSegmentSeatMap == null)
            {
                return Result.Success
                    (
                        new Response<SeatAvailabilityResponse>
                        {
                            Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.InvalidOfferRefId, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                            Status = Status.BusinessException
                        }
                    );
            }

            return Result.Success(new Response<SeatAvailabilityResponse>
            {
                Status = Status.OK,
                Value = seatOfferResponse
            });
        }
    }
}
