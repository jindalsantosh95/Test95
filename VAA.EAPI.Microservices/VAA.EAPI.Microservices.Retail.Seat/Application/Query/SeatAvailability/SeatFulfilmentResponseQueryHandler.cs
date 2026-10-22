using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Common;

namespace VAA.EAPI.Microservices.Retail.Seat.Application.Query.SeatAvailability
{
    public class SeatFulfilmentResponseQueryHandler : IRequestHandler<SeatFulfilmentResponseQuery, Result<Response<SeatFulfilmentResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SeatFulfilmentResponseQueryHandler> _logger;

        public SeatFulfilmentResponseQueryHandler(IMediator mediator, ILogger<SeatFulfilmentResponseQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<SeatFulfilmentResponse>>> Handle(SeatFulfilmentResponseQuery request, CancellationToken cancellationToken)
        {
            Response<SeatFulfilmentResponse> seatFulfilmentrResponse = new();

            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);

                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
                {
                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = request.Type }, cancellationToken);

                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                    switch (result.Status)
                    {
                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:

                            await Task.Delay(timespan, cancellationToken);
                            break;

                        case ServiceResponseProcessStatus.Completed:

                            seatFulfilmentrResponse = new Response<SeatFulfilmentResponse>()
                            {
                                Status = Status.OK,
                                Value = Json.Deserialize<SeatFulfilmentResponse>(result.JsonResponseData)
                            };
                            responseProcessed = true;
                            break;

                        case ServiceResponseProcessStatus.Error:

                            return Result.Failure<Response<SeatFulfilmentResponse>>(result.JsonResponseData);

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    retryCounter++;
                }

                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);

                return seatFulfilmentrResponse.Value != null || seatFulfilmentrResponse.Errors != null ? Result.Success(seatFulfilmentrResponse) : Result.Failure<Response<SeatFulfilmentResponse>>(Json.Serialize(errorResponse));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex}");
                return Result.Failure<Response<SeatFulfilmentResponse>>(ex.Message);
            }
        }
    }
}
