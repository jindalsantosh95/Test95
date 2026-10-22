using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment
{
    public class OrderPaymentEventCommandHandler : IRequestHandler<OrderPaymentEventCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderPaymentEventCommandHandler> _logger;

        public OrderPaymentEventCommandHandler(IMediator mediator, ILogger<OrderPaymentEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<Result<Response<string>>> Handle(OrderPaymentEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                bool mockEnabled = ConfigReader.Try<bool>(Constants.Environment.MockOrderPaymentService);
                var result = new Validation.ValidateOrderPaymentRequest().Validate(request);
                var id = string.Empty;

                if (result.IsValid)
                {
                    if (!mockEnabled)
                    {
                        var cloudEvent = OrderPaymentUpdateCommandToEventMapper.CommandToEventMapper(request);

                        id = cloudEvent.Id;

                        await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject }, cancellationToken);

                        try
                        {
                            await _mediator.Publish(cloudEvent, cancellationToken);

                            var orderPaymentUpdateResponse = OrderPaymentUpdateEventCommandResponseMapper.OrderPaymentResponseMapper(cloudEvent.Id, Status.Accepted);

                            var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, cloudEvent.Id, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);

                            return orderPaymentUpdateResponse != null ? Result.Success(orderPaymentUpdateResponse) : Result.Failure<Response<string>>(Json.Serialize(errorResponse));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Rollback from event store for CorrelationId: {AsyncContext.Current.CorrelationId} due to error in OrderPaymentEventCommand handler");

                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = id, Type = EnumOperations.GetEnumDescription((PaymentEventType)Enum.Parse(typeof(PaymentEventType), PaymentEventType.OrderPaymentUpdateInitiate.ToString())), Data = request, Subject = Constants.Application.OrderPaymentUpdateEventSubject }, cancellationToken);

                            var error = new Response<string>()
                            {
                                Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.ServiceBusError, id, EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                            }; throw new MessageBrokerException($"{Json.Serialize(error.Errors)}");
                        }
                    }
                    else
                    {
                        return await Task.FromResult(Result.Success(OrderPaymentUpdateEventCommandResponseMapper.OrderPaymentResponseMapper(id, Status.Accepted)));
                    }
                }
                else
                {
                    var errorCode = Convert.ToString(result.Errors.FirstOrDefault());

                    _logger.LogWarning($"Order Payment Request Validation Failed due to error: {errorCode} for CorrelationId: {AsyncContext.Current.CorrelationId}");

                    return Result.Success(new Response<string>
                    {
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                        Status = Status.BadRequest
                    });
                }
            }
            catch (MessageBrokerException ex)
            {
                _logger.LogError(ex, ex.Message);

                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);

                return Result.Failure<Response<string>>(Json.Serialize(errorResponse));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in OrderPaymentEventCommand for CorrelationId: {AsyncContext.Current.CorrelationId}");

                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);

                return Result.Failure<Response<string>>(Json.Serialize(errorResponse));
            }
        }

    }
}
