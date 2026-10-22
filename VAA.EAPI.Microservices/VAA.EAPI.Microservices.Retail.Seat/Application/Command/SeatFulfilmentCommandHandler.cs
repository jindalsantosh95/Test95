using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Seat.Constants;
using VAA.EAPI.Microservices.Retail.Seat.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Seat.Mapper;
using VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Validations;

namespace VAA.EAPI.Microservices.Retail.Seat.Application.Command
{
    public class SeatFulfilmentCommandHandler : IRequestHandler<SeatFulfilmentCommand, Result<Response<string>>>
    {

        private readonly IMediator _mediator;
        private readonly ILogger<SeatFulfilmentCommandHandler> _logger;
        private readonly ICachedChannel _cachedChannel;

        public SeatFulfilmentCommandHandler(IMediator mediator, ILogger<SeatFulfilmentCommandHandler> logger, ICachedChannel cachedChannel)
        {
            _mediator = mediator;
            _logger = logger;
            _cachedChannel = cachedChannel;
        }

        public async Task<Result<Response<string>>> Handle(SeatFulfilmentCommand request, CancellationToken cancellationToken)
        {
            bool eventCreated = false;
            var id = Guid.NewGuid().ToString();

            try
            {
                var channelEntity = await _cachedChannel.GetChannelEntity(request.ChannelId);

                if (channelEntity != null)
                {
                    var cloudEvent = CloudEventMapper.Map(request);

                    id = cloudEvent.Id;

                    var result = SeatFulfilmentRequestValidator.ValidateRequest(request);

                    if (result.Value.Status == Status.OK)
                    {
                        await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject }, cancellationToken);

                        eventCreated = true;

                        await _mediator.Publish(cloudEvent, cancellationToken);

                        var seatFulfilmentResponse = SeatFulfilmentEventCommandResponseMapper.SeatFulfilmentResponseMapper(cloudEvent.Id, Convert.ToString(HttpStatusCode.Accepted));

                        return seatFulfilmentResponse != null ? Result.Success(seatFulfilmentResponse) : Result.Failure<Response<string>>(Convert.ToString(HttpStatusCode.InternalServerError));
                    }
                    else
                    {
                        return Result.Success(new Response<string>
                        {
                            Errors = result.Value.Errors,
                            Status = Status.BusinessException
                        });
                    }
                }
                else
                {
                    return Result.Success(new Response<string>
                    {
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.InvalidChannelErrorCode, id, EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                        Status = Status.BusinessException
                    });

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex}");
                if (eventCreated)
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = id, Type = SeatFulfilmentEventType.SeatFulfilmentInitiate.ToString(), Data = request, Subject = Constants.Application.SeatFulfilmentEventSubject });

                return Result.Failure<Response<string>>(ex.Message);

            }
        }
    }
}
