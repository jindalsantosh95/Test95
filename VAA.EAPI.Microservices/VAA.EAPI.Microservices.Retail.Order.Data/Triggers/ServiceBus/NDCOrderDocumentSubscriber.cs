using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCOrderStore;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.ServiceBus
{
    public class NDCOrderDocumentSubscriber
    {
        private readonly IMediator _mediator;
        
        public NDCOrderDocumentSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));            
        }

        [Function("NDCOrderStoreEventSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Environment.NDCOrderStoreEventHandlerTopicName, Constants.Environment.NDCOrderStoreSubscriptionName, Connection = "OrderServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<NDCOrderStoreRequestCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
