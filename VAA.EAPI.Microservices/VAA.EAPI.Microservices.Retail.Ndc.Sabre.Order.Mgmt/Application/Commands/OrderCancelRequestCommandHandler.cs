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
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Query;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands
{
    public class OrderCancelRequestCommandHandler : IRequestHandler<OrderCancelRequestCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderCancelRequestCommandHandler> _logger;

        public OrderCancelRequestCommandHandler(IMediator mediator, ILogger<OrderCancelRequestCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(OrderCancelRequestCommand request, CancellationToken cancellationToken)
        {

            var id = string.Empty;
            try
            {
                if (request.OrderChangeRequestData == null) throw new ArgumentNullException(nameof(request.OrderChangeRequestData));

                var orderCancelCloudEvent = OrderCancelDataMapper.EventMapper(request.OrderChangeRequestData);

                id = orderCancelCloudEvent.Id;

                await _mediator.Send(new EventRequestInsertCommand { Id = orderCancelCloudEvent.Id, Type = orderCancelCloudEvent.Type, Data = orderCancelCloudEvent.Data, Subject = orderCancelCloudEvent.Subject }, cancellationToken);

                try
                {
                    await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = orderCancelCloudEvent, ServiceBusTopicName = Constants.Application.NDCCancelPnrTopicName, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString) }, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Order cancellation event processing failed for {EventId}", orderCancelCloudEvent.Id);

                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = orderCancelCloudEvent.Id, Type = orderCancelCloudEvent.Type, Data = orderCancelCloudEvent.Data, Subject = orderCancelCloudEvent.Subject }, cancellationToken);

                    throw;
                }

                var response = OrderCancelDataMapper.ResponseMapper(orderCancelCloudEvent.Id);

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order cancellation request handling failed for {EventId}", id);

                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, id, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);

                return Result.Failure<Response<string>>(Json.Serialize(errorResponse));
            }
        }
    }
}



