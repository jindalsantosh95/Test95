using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Create.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Create.Triggers.ServiceBus
{
    public class OrderCreateSubscriber
    {
        private readonly IMediator _mediator;
        public OrderCreateSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("OrderCreateSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.OrderCreateTopicName, Constants.Application.OrderCreateSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            var orderCreateEventRequest = OrderCreateEventDataMapper.RequestMapper(cloudEvent);

            await _mediator.Send(orderCreateEventRequest);
        }
    }
}
