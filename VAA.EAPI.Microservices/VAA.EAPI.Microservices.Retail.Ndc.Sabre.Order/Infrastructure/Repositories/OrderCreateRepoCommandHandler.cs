using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Infrastructure.Repositories
{
    public class OrderCreateRepoCommandHandler : IRequestHandler<OrderCreateRepoCommand, Result<Response<GetReservationRS>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderCreateRepoCommandHandler> _logger;

        public OrderCreateRepoCommandHandler(IMediator mediator, ILogger<OrderCreateRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<GetReservationRS>>> Handle(OrderCreateRepoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //Create Sabres session
                var token = await _mediator.Send(new CreateSessionCommand() { PCC = request.OrderCreateRequest.Parties.Participants.First().PccCode }, cancellationToken);

                GetReservationRS getReservationresponse = null;

                if (token != null)
                {
                    var cpnrRequest = CreatePassengerNameRecordRQMapper.GetCPNRRequest(token, request.OrderCreateRequest);
                    var createPnrResponse = await _mediator.Send(new CreatePNRCommand() { CreatePNRRequest = cpnrRequest }, cancellationToken);

                    if (createPnrResponse.ApplicationResults?.Error != null)
                    {
                        _logger.LogError("Sabre operation returned errors. {Operation} {ErrorCount}", "CreatePNR", createPnrResponse.ApplicationResults.Error.Length);
                    }

                    bool checkUcStatus = createPnrResponse?.AirBook?.OriginDestinationOption.Any(x => x.Status == "UC") ?? false;

                    if (checkUcStatus)
                    {
                        return Result.Failure<Response<GetReservationRS>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.InValidSegmentError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                    }

                    if (createPnrResponse.ItineraryRef != null)
                    {
                        var pollResult = await PollForValidAir4PnrAsync( token, createPnrResponse.ItineraryRef.ID, cancellationToken);

                        getReservationresponse = pollResult.Response;

                        if (!pollResult.IsSuccess)
                        {
                            var recordLocator = (getReservationresponse?.Item as ReservationPNRB)?.BookingDetails?.RecordLocator;

                            var errors = ErrorMapper.MapTechnicalExceptionToEAPIException( Constants.ErrorCodes.MissingAir4PNR, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);

                            if (errors?.Any() == true)
                            {
                                errors[0].DeveloperMessage +=  $" | RecordLocator: {recordLocator}";
                            }

                            return Result.Failure<Response<GetReservationRS>>(Json.Serialize(errors));
                        }

                        // close sabre session
                        await _mediator.Send(new SessionCloseCommand() { BinaryToken = token, PCC = request.OrderCreateRequest.Parties.Participants.First().PccCode });

                        bool actionCode = ((getReservationresponse.Item as ReservationPNRB).PassengerReservation.Segments.Segment.Any(x => (x.Item as SegmentTypePNRBSegmentAir)?.ActionCode == "UC"));

                        if (actionCode)
                        {
                            return Result.Failure<Response<GetReservationRS>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.InValidSegmentError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                        }

                        if (getReservationresponse != null && getReservationresponse.Errors == null)
                        {
                            return Result.Success(new Response<GetReservationRS>
                            {
                                Status = Status.OK,
                                Value = getReservationresponse
                            });
                        }

                    }

                    // close sabre session
                    await _mediator.Send(new SessionCloseCommand() { BinaryToken = token, PCC = request.OrderCreateRequest.Parties.Participants.First().PccCode }, cancellationToken);

                    if (createPnrResponse.ApplicationResults?.Warning != null)
                    {
                        _logger.LogWarning("Sabre operation returned warnings. {Operation}", "CreatePNR");

                        if (createPnrResponse.ApplicationResults.Warning.SelectMany(x => x.SystemSpecificResults.SelectMany(x => x.Message.Select(x => x.Value))).Any(x => x.Contains("*IM AND CANCEL UNABLE SEGMENTS")))
                        {
                            return Result.Failure<Response<GetReservationRS>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.InValidSegmentError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                        }

                        if (!createPnrResponse.ApplicationResults.Warning.SelectMany(w => w.SystemSpecificResults).Where(m => m.Message.Any(x => x.Value.Contains("SpecialServiceLLSRQ"))).IsNullOrEmpty())
                            request.OrderCreateRequest.DataList.OrderInstructions.First().Status = "Invalid";
                    }
                }
                else
                {
                    throw new Exception("Failure occurred while generating token in OrderCreate Repo Command Handler");
                }

                return Result.Failure<Response<GetReservationRS>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (ExternalServiceIntegrationException)
            {
                _logger.LogError("OrderCreate processing failed. {Operation}", "OrderCreate");
                return Result.Failure<Response<GetReservationRS>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OrderCreate processing failed. {Operation}", "OrderCreate");
                return Result.Failure<Response<GetReservationRS>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }

        private async Task<(GetReservationRS Response, bool IsSuccess)> PollForValidAir4PnrAsync(string token,string itineraryId,CancellationToken cancellationToken)
        {
            int responseDelayInMS = ConfigReader.Try(Constants.Environment.PollingIntervalMs, 1000);
            int maxRetries = ConfigReader.Try(Constants.Environment.MaxAttempts, 8);

            GetReservationRS reservationResponse = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                var getReservationRequest = GetReservationRQMapper.GetReservationRequestMapper(token,itineraryId);
                reservationResponse = await _mediator.Send( new GetReservationQuery(getReservationRequest),cancellationToken);

                if (reservationResponse?.Errors == null && !HasInvalidAir4Pnr(reservationResponse))
                {
                    return (reservationResponse, true);
                }

                if (attempt < maxRetries)
                {
                    _logger.LogWarning(
                        "Sabre GetReservation polling attempt did not return a valid reservation. {Operation} {Attempt} {AttemptCount}",
                        "GetReservation",
                        attempt,
                        maxRetries);
                    await Task.Delay(responseDelayInMS, cancellationToken);
                }
            }

            _logger.LogError("Sabre operation failed after polling attempts. {Operation} {AttemptCount}", "GetReservation", maxRetries);
            return (reservationResponse, false);
        }

        private static bool HasInvalidAir4Pnr(GetReservationRS response)
        {
            return ((response?.Item as ReservationPNRB)?.PassengerReservation?.Segments?.Segment?.Any(x =>
                {
                    var airSeg = x.Item as SegmentTypePNRBSegmentAir;
                    return airSeg?.AirlineRefId?.Equals("DCVS*", StringComparison.OrdinalIgnoreCase) == true;
                }) ?? false);
        }
    }
}
