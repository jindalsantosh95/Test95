using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Triggers.ServiceBus
{
    public class ManageSpecialServiceMessageSubscriber
    {
        private readonly IMediator _mediator;

        public ManageSpecialServiceMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("ManageSpecialServiceMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.OrderChangeEventHandlerTopicName, Constants.Application.ManageSpecialServiceEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var ssrEventProcessCommand = Json.Deserialize<ManageSpecialServiceEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(ssrEventProcessCommand);

            await _mediator.Send(ssrEventProcessCommand);
        }
    }
}
