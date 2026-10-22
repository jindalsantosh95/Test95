using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Notification.Constants;
using VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Notification.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Notification.Validations;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Application.Commands
{
    public class OrderNotificationRequestCommandHandler : IRequestHandler<OrderNotificationRequestCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderNotificationRequestCommandHandler> _logger;

        public OrderNotificationRequestCommandHandler(IMediator mediator, ILogger<OrderNotificationRequestCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(OrderNotificationRequestCommand request, CancellationToken cancellationToken)
        {
            var id = string.Empty;

            try
            {
                _logger.LogInformation($"OrderNotificationRequestCommandHandler | CorrelationId: {AsyncContext.Current.CorrelationId}");
                //Map request to cloud event
                CloudEvent cloudEvent = OrderNotificationCommandToEventMapper.CommandToEventMapper(request);

                id = cloudEvent.Id;

                if (!(ConfigReader.Get(Constants.Environment.MockResponseEnabled) == "Y"))
                {
                    //Validate the request
                    var validationStatus = await new OrderNotificationRequestCommandValidator()
                                                    .ValidateAsync(
                                                                    instance: request,
                                                                    cancellation: cancellationToken
                                                                   );
                    if (!validationStatus.IsValid)
                    {
                        var errorMessage = validationStatus.Errors.LastOrDefault().ErrorMessage;
                        _logger.LogWarning($"OrderNotificationRequestCommandHandler | Valdiation error code :{errorMessage} ");
                        return new Response<string>()
                        {
                            Status = Status.BadRequest,
                            Errors = ErrorMapper.MapBusinessExceptionToEAPIException
                                        (
                                            ex: new Exception(errorMessage),
                                            guid: id,
                                            errorFilePath: Infrastructure.Constants.Constants.ErrorCodesPath
                                        )
                        };
                    }

                    //Store in event store
                    _logger.LogDebug($"Store event in event store - Start");
                    await _mediator.Send(new EventRequestInsertCommand
                    {
                        Id = cloudEvent.Id,
                        Type = cloudEvent.Type,
                        Data = cloudEvent.Data,
                        Subject = cloudEvent.Subject
                    }
                    , cancellationToken);

                    _logger.LogDebug($"Store event in event store - End");

                    //publish to service bus      
                    _logger.LogDebug($"Publish event - Start");
                    try
                    {
                        await _mediator.Publish(cloudEvent, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Rollback from event store due to error, ErroID: {id}");
                        await _mediator.Send(new EventStoreRequestDeleteCommand
                        {
                            Id = id,
                            Type = EnumOperations.GetEnumDescription
                            (
                            value: (OrderNotificationTypes)Enum
                                    .Parse
                                    (
                                        enumType: typeof(OrderNotificationTypes),
                                        value: OrderNotificationTypes.OrderNotificationInitiate.ToString()
                                    )
                           ),
                            Data = request,
                            Subject = Constants.Application.OrderNotificationEventSubject
                        }
                        , cancellationToken);

                        var error = new Response<string>()
                        {
                            Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.ServiceBusError,id, Infrastructure.Constants.Constants.ErrorCodesPath)
                        }; throw new MessageBrokerException($"{Json.Serialize(error.Errors)}");
                    }
                    _logger.LogDebug($"Publish event - End");

                }
                var response = OrderNotificationEventCommandResponseMapper.OrderNotificationResponseMapper
                                (
                                    correlationid: cloudEvent.Id,
                                    status: Convert.ToString(HttpStatusCode.Accepted)
                                );

                return Result.Success(response);
            }
            catch (MessageBrokerException ex)
            {
                _logger.LogError(ex.Message);
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException
                                                    (
                                                        errorCode: Infrastructure.Constants.Constants.TechnicalError,
                                                        guid: id,
                                                        errorFilePath: Infrastructure.Constants.Constants.GlobalExceptionFilePath
                                                    );
                return Result.Failure<Response<string>>(Json.Serialize(errorResponse));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in OrderNotificationRequestCommandHandler for CorrelationId: {AsyncContext.Current.CorrelationId}");
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException
                                    (
                                        errorCode: Infrastructure.Constants.Constants.TechnicalError,
                                        guid: id,
                                        errorFilePath: Infrastructure.Constants.Constants.GlobalExceptionFilePath
                                    );
                return Result.Failure<Response<string>>(Json.Serialize(errorResponse));
            }
        }
    }
}
