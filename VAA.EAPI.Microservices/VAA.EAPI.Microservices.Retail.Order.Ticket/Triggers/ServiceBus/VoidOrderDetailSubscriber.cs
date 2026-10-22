using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Triggers.ServiceBus
{
    public class VoidOrderDetailSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public VoidOrderDetailSubscriber(IMediator mediator, ILogger<VoidOrderDetailSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("VoidOrderDetailSubscriber")]
        public async Task Run([ServiceBusTrigger(topicName: "%ServiceBusTopicName%", subscriptionName: "%VoidOrderDetailSubscriptionName%", Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<VoidOrderDetailEventProcessCommand>(eventRequestPayload);

            _logger.LogInformation($"VoidOrderDetailSubscriber | Correlational ID :  {cloudEvent.Id}");

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent, CancellationToken.None);
        }
    }
}
