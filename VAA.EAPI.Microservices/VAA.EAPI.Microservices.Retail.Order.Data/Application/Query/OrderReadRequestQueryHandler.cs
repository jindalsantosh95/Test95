using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query
{
    public class OrderReadRequestQueryHandler : IRequestHandler<OrderReadRequestQuery, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderReadRequestQueryHandler> _logger;

        public OrderReadRequestQueryHandler(IMediator mediator, ILogger<OrderReadRequestQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Unit>> Handle(OrderReadRequestQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));
                var req = Json.DeserializeObject<OrderReadRequestData>((JToken)request.Data);

                #region Read Document from Cosmos
                var voidOrderDetails = await _mediator.Send(new OrderReadRepositoryCommand(req), cancellationToken);
                #endregion

                #region Publish Event for Ticket MS
                var orderDetails = VoidOrderDetailsMapper.MapVoidOrderDetails(voidOrderDetails, req);
                var eventData = VoidOrderEventMapper.OrderDetailMapper(orderDetails, request);
                bool checkEventStored = false;

                try
                {
                    await _mediator.Send(new EventRequestInsertCommand { Id = eventData.Id, Type = eventData.Type, Data = eventData.Data, Subject = eventData.Subject });
                    checkEventStored = true;
                    await _mediator.Publish(eventData, CancellationToken.None);
                }
                catch (Exception exception)
                {
                    if (checkEventStored)
                    {
                        _logger.LogWarning(exception, "Rolling back Order Read event store record after Ticket event publish failure with exception type {ExceptionType} for event {EventId}",
                            exception.GetType().Name, eventData.Id);
                        await _mediator.Send(new EventStoreRequestDeleteCommand { Id = eventData.Id, Type = eventData.Type, Data = eventData.Data, Subject = eventData.Subject });
                    }
                    var error = new Response<string>()
                    {
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.ServiceBusError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    }; throw new MessageBrokerException($"{Json.Serialize(error.Errors)}");
                }
                #endregion

            }
            catch (MessageBrokerException ex)
            {
                _logger.LogError(ex, "Order Read Ticket event publish failed with error code {ErrorCode} for event {EventId}", ErrorCodes.ServiceBusError, request?.Id); throw;
            }
            catch (CosmosDBException ex)
            {
                _logger.LogError(ex, "Order Read Cosmos DB query failed with error code {ErrorCode} for event {EventId}", ErrorCodes.CosmosErorrCode, request?.Id); throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order Read failed with exception type {ExceptionType} for event {EventId}",
                    ex.GetType().Name, request?.Id); throw;
            }
            return Unit.Value;
        }
    }
}
