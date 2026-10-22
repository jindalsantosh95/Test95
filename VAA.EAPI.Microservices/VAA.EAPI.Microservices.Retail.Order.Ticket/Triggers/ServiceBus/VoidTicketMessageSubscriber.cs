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
    public class VoidTicketMessageSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public VoidTicketMessageSubscriber(IMediator mediator, ILogger logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("VoidTicketMessageSubscriber")]
        public async Task Run([ServiceBusTrigger("%OrderTicketVoidEventHandlerTopicName%", "%OrderTicketVoidEventSubscriptionName%", Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            _logger.LogInformation("VoidTicketMessage received");

            var cloudEvent = Json.Deserialize<VoidTicketEventCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            _logger.LogInformation($"VoidTicketMessageSubscriber | Correlational ID :  {cloudEvent.Id}");
            
            await _mediator.Send(cloudEvent, CancellationToken.None);
        }
    }
}
