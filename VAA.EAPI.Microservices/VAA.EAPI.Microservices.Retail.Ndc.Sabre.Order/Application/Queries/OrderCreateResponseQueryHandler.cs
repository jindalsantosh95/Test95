using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Application.Queries
{
    public class OrderCreateResponseQueryHandler : IRequestHandler<OrderCreateResponseQuery, Result<Response<OrderDocument>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderCreateResponseQueryHandler> _logger;

        public OrderCreateResponseQueryHandler(IMediator mediator, ILogger<OrderCreateResponseQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<OrderDocument>>> Handle(OrderCreateResponseQuery request, CancellationToken cancellationToken)
        {
            Response<OrderDocument> orderResponse = new();

            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMs = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);

                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
                {
                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = request.Type }, cancellationToken);
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMs * retryCounter);

                    switch (result.Status)
                    {
                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:
                            await Task.Delay(timespan, cancellationToken);
                            break;

                        case ServiceResponseProcessStatus.Completed:
                            orderResponse = new()
                            {
                                Status = Status.OK,
                                Value = Json.Deserialize<OrderDocument>(result.JsonResponseData)
                            };
                            responseProcessed = true;

                            await _mediator.Send(new ServiceBusTopicMessageEventCommand() { Message = new EAPI.Domain.Events.CloudEvent() { Id = request.CorrelationId } , ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString) , ServiceBusTopicName = Constants.Application.DeleteOrderEventStoreHandlerTopicName});

                            break;

                        case ServiceResponseProcessStatus.Error:
                            return Result.Failure<Response<OrderDocument>>(result.JsonResponseData);

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    retryCounter++;
                }

                return orderResponse.Value != null ? Result.Success(orderResponse) : Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OrderCreate response retrieval failed. {EventId}", request.CorrelationId);

                return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
