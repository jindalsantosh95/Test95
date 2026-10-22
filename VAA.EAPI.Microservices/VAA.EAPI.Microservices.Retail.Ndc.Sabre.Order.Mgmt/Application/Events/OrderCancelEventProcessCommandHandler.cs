using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Events
{
    public class OrderCancelEventProcessCommandHandler : IRequestHandler<OrderCancelEventProcessCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;        
        private readonly ILogger<OrderCancelEventProcessCommandHandler> _logger;

        public OrderCancelEventProcessCommandHandler(IMediator mediator, ILogger<OrderCancelEventProcessCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(OrderCancelEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orderChangeRequestData = Json.Deserialize<OrderChangeRequestData>(request.Data.ToString());
                var response = await _mediator.Send(new OrderCancelRepoCommand() { orderChangeRequestData = orderChangeRequestData}, cancellationToken);

                if(response.IsSuccess)
                {
                    var OrderCancelCloudEvent = OrderCancelDataMapper.EventMapper(orderChangeRequestData, request.Id);
                    await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = OrderCancelCloudEvent, ServiceBusTopicName = Constants.Application.NDCCancelPnrTopicName, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderDataServiceBusConnectionString) }, cancellationToken);

                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(response), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.OrderCancelEventSubject }, cancellationToken);
                }
                else
                {
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(response), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.OrderCancelEventSubject }, cancellationToken);

                    throw new Exception("Exception while cancelling the PNR");
                }               

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order cancellation event processing failed for {EventId}", request.Id);
            }

            return Unit.Value;
        }        
    }
}
