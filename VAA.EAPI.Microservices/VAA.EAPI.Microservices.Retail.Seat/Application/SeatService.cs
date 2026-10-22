using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Seat.Application.Command;
using VAA.EAPI.Microservices.Retail.Seat.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Seat.Domain.Seats;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper.Interface;
using Environment = VAA.EAPI.Microservices.Retail.Seat.Constants.Environment;
using VAA.EAPI.Common;
using VAA.EAPI.Microservices.Retail.Seat.Constants;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Seat.Mapper;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Application
{
    public class SeatService : ISeatService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SeatService> _logger;
        private readonly ICachedChannel _cachedChannel;

        public SeatService(IMediator mediator, ILogger<SeatService> logger, ICachedChannel cachedChannel)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cachedChannel = cachedChannel;
        }

        public async Task<Result<Response<string>>> DeleteSeatAsync(SeatDeletionRequest request)
        {
            bool eventCreated = false;
            var id = string.Empty;

            try
            {
                var channelInfo = await _cachedChannel.GetChannelEntity(request.ChannelId);

                if (channelInfo != null)
                {
                    var cloudEvent = CloudEventMapper.Map(request);

                    id = cloudEvent.Id;

                    await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject }, CancellationToken.None);

                    eventCreated = true;

                    await _mediator.Publish(cloudEvent, CancellationToken.None);

                    var seatDeletionResponse = new Response<string>
                    {
                        Status = Status.Accepted,
                        HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaaderParameterlocation, Value = ConfigReader.Get(Environment.SeatDeletionResponseUrl).Replace("{correlationid}", id) } },
                    };

                    return seatDeletionResponse != null ? Result.Success(seatDeletionResponse) : Result.Failure<Response<string>>(Convert.ToString(HttpStatusCode.InternalServerError));
                }
                else
                {
                    return Result.Success(new Response<string>
                    {
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.InvalidChannelErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                        Status = Status.BusinessException
                    });

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex}");

                if (eventCreated)
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = id, Type = SeatFulfilmentEventType.SeatDeletionInitiate.ToString(), Data = request, Subject = Constants.Application.SeatDeletionEventSubject });

                return Result.Failure<Response<string>>(ex.Message);
            }
        }

        public async Task<Result<Response<SeatDeletionResponse>>> GetDeleteSeatResponseAsync(string id, string type)
        {
            Response<SeatDeletionResponse> seatDeletionResponse = new();

            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);

                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
                {
                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = id, EventType = type }, CancellationToken.None);

                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                    switch (result.Status)
                    {
                        case ServiceResponseProcessStatus.Processing:

                            await Task.Delay(timespan, CancellationToken.None);

                            break;
                        case ServiceResponseProcessStatus.Completed:

                            seatDeletionResponse = new Response<SeatDeletionResponse>()
                            {
                                Status = Status.OK,
                                Value = Json.Deserialize<SeatDeletionResponse>(result.JsonResponseData)
                            };
                            responseProcessed = true;

                            break;
                        case ServiceResponseProcessStatus.Error:

                            return Result.Failure<Response<SeatDeletionResponse>>(result.JsonResponseData);
                    }

                    retryCounter++;
                }

                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);

                return seatDeletionResponse.Value != null || seatDeletionResponse.Errors != null ? Result.Success(seatDeletionResponse) : Result.Failure<Response<SeatDeletionResponse>>(Json.Serialize(errorResponse));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex}");

                return Result.Failure<Response<SeatDeletionResponse>>(ex.Message);
            }
        }

    }
}
