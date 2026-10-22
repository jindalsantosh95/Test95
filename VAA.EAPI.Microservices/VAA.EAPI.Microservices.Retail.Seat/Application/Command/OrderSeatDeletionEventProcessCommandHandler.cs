using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Application.Command
{
    public class OrderSeatDeletionEventProcessCommandHandler(IMediator mediator, ILogger<OrderSeatDeletionEventProcessCommandHandler> logger) : IRequestHandler<OrderSeatDeletionEventProcessCommand, Result<Unit>>
    {
        private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        private readonly ILogger<OrderSeatDeletionEventProcessCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<Result<Unit>> Handle(OrderSeatDeletionEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
               
                var seatDelRequest = Json.Deserialize<OrderSeatV8DeleteSeatCommand>(request.Data.ToString());
              
                var sessionId = await _mediator.Send(new PNRSetupContextRepoQuery() { RecordLocator = seatDelRequest.ExternalRloc , AssignPrinter = false }, cancellationToken);

                seatDelRequest.SessionId = sessionId;
                var response = await _mediator.Send(seatDelRequest, cancellationToken);

                if (response.IsNullOrDefault()) { throw new NotSupportedException(); }
                await _mediator.Send(new PnrTeardownContextRepoCommand() { SessionId = sessionId} , cancellationToken);

                if (response?.Deletion == true && seatDelRequest.ChannelId == Constants.Application.NdcChannel)
                {
                    var cloudEvent = SeatDeletionCommandToEventMapper.CommandToEventMapper(response, request.Id, ConfigReader.Get(Constants.Environment.OrderSeatDeleteServiceBusTopicName));
                    await _mediator.Publish(cloudEvent, cancellationToken);


                    await _mediator.Send(new ServiceBusTopicMessageEventCommand() { Message = SeatDeletionCommandToEventMapper.CommandToEventMapper(response, request.Id, Constants.Application.NDCUpdateSeatTopicName), ServiceBusTopicName = Constants.Application.NDCUpdateSeatTopicName, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderServiceBusConnectionString) }, cancellationToken);
                }                    

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(response), Status = ServiceResponseProcessStatus.Completed, Type = request.Type }, cancellationToken);
            }
            catch (ExternalServiceIntegrationException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex}");

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(Constants.ErrorCodes.DeltaSeatV8ErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message)), Status = ServiceResponseProcessStatus.Error, Type = request.Type }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex}");

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.ExceptionErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)), Status = ServiceResponseProcessStatus.Error, Type = request.Type }, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
