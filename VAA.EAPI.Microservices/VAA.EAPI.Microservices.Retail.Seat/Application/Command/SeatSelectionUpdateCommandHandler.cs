using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Seat.Constants;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Application.Command
{
    public class SeatSelectionUpdateCommandHandler : IRequestHandler<SeatSelectionUpdateCommand, Result<Response<SeatAvailabilityResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SeatSelectionUpdateCommandHandler> _logger;

        public SeatSelectionUpdateCommandHandler(IMediator mediator, ILogger<SeatSelectionUpdateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<Result<Response<SeatAvailabilityResponse>>> Handle(SeatSelectionUpdateCommand requestData, CancellationToken cancellationToken)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            if (requestData.SeatOfferID == null) throw new ArgumentNullException(nameof(requestData.SeatOfferID));
            try
            {
                var seatOfferResponse = await _mediator.Send(new SeatOfferSearchRepoQuery(requestData.SeatOfferID), cancellationToken);

                if (seatOfferResponse.FlightSegmentSeatMap == null)
                {
                    return Result.Success
                        (
                            new Response<SeatAvailabilityResponse>
                            {
                                Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.InvalidOfferRefId, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                                Status = Status.BusinessException
                            }
                        );
                }

                SeatAvailabilityResponse seatAvailabilityResponse = new()
                {
                    SeatOfferId = seatOfferResponse.SeatOfferId,
                    FlightSegmentSeatMap = SeatMapResponseSeatSelectionMapper.MapSeatSelect(seatOfferResponse.FlightSegmentSeatMap, requestData.PassengerSeats),
                    OrderId = seatOfferResponse.OrderId,
                    Parties = seatOfferResponse.Parties,
                    Pos = seatOfferResponse.Pos
                };

                return new Response<SeatAvailabilityResponse>
                {
                    Status = Status.OK,
                    Value = seatAvailabilityResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error has occurred in Seat Selection Update Command Handler");
                return Result.Failure<Response<SeatAvailabilityResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
