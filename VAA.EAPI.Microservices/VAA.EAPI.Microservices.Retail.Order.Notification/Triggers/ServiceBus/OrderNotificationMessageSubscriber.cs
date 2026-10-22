using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Notification.Application.Commands;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Triggers.ServiceBus
{
    public class OrderNotificationMessageSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public OrderNotificationMessageSubscriber(IMediator mediator, ILogger<OrderNotificationMessageSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("OrderNotificationMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(topicName: "%ServiceBusTopicName%", subscriptionName: "%ServiceBusSubscriptionName%", Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<OrderNotificationEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            _logger.LogInformation($"OrderNotificationMessageSubscriber | Correlational ID :  {cloudEvent.Id}");

            await _mediator.Send(cloudEvent, CancellationToken.None);
        }
    }
}
