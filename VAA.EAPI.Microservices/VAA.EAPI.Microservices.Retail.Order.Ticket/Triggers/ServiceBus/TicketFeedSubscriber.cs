using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands.UpsertTicket;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Triggers.ServiceBus
{
    public class TicketFeedSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TicketFeedSubscriber> _logger;

        public TicketFeedSubscriber(IMediator mediator, ILogger<TicketFeedSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("TicketFeedSubscriber")]
        public async Task UpsertTicketFeed([ServiceBusTrigger("%ticket-ms-queue%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, FunctionContext context)
        {
            try
            {
                var cloudEvent = Json.Deserialize<CloudEvent>(message.Body.ToString());

                AsyncContext.Current.MapFromCloudEvent(cloudEvent);

                await _mediator.Send(new UpsertTicketCommand { TicketFeedEventData = message.Body.ToString() }, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ProcessMessageAsync: Error processing message: {message.MessageId}, Exception: {ex.Message}");
                throw;
            }
        }
    }
}
