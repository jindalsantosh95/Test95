using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Events;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Triggers.ServiceBus
{
    public class SplitPnrMessageSubscriber
    {
        private readonly ISplitPnrEventProcessCommand _splitPnrEventProcessCommand;
        private readonly CancellationToken _cancellationToken;


        public SplitPnrMessageSubscriber(ISplitPnrEventProcessCommand splitPnrEventProcessCommandHandler)
        {
            _splitPnrEventProcessCommand = splitPnrEventProcessCommandHandler ?? throw new ArgumentNullException(nameof(splitPnrEventProcessCommandHandler));
            _cancellationToken = new CancellationToken();
        }

        [Function("SplitPnrMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.SplitPnrEventHandlerTopicName, Constants.Application.SplitPnrEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var eventRequest = Json.Deserialize<CloudEvent>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(eventRequest);

            await _splitPnrEventProcessCommand.Handle(eventRequest, _cancellationToken);
        }
    }
}
