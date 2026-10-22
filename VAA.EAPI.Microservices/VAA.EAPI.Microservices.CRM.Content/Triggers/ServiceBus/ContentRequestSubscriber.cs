using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Microservices.CRM.Content.Interfaces;
using VAA.EAPI.Microservices.CRM.Content.Models;

namespace VAA.EAPI.Microservices.CRM.Content.Triggers.ServiceBus
{
    public class ContentRequestSubscriber
    {
        private readonly IContentRequestHandler _contentRequestHandler;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<ContentRequestSubscriber> _logger;

        public ContentRequestSubscriber(IContentRequestHandler contentRequestHandler, IMessageBus messageBus, ILogger<ContentRequestSubscriber> logger)
        {
            _contentRequestHandler = contentRequestHandler ?? throw new ArgumentNullException(nameof(contentRequestHandler));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("ContentRequestSubscriber")]
        public async Task Run([
            ServiceBusTrigger(Constants.Application.ContentRequestTopicName, Constants.Application.ContentRequestTopicSubscription, Connection = Constants.Environment.ContentServiceBusConnection)
        ] string eventRequestPayload, CancellationToken cancellationToken)
        {
            // Deserialize the incoming message to a CloudEvent
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            AsyncContext.Current?.MapFromCloudEvent(cloudEvent);

            // Deserialize the CloudEvent data to a ContentRequestEvent
            var contentRequestEvent = Json.Deserialize<ContentRequestEvent>(cloudEvent.Data.ToString());

            // Process the content request
            var content = await _contentRequestHandler.Handle(contentRequestEvent.ContentFragmentId?.ToString(), cancellationToken);
            var itemsData = content.SelectToken("items")?.Select(Newtonsoft.Json.JsonConvert.SerializeObject).ToList();

            // Send it back to the reply topic if the reply topic and subject are provided
            if (!string.IsNullOrEmpty(contentRequestEvent.ReplyTopic) && !string.IsNullOrEmpty(contentRequestEvent.ReplySubject))
            {
                var responseEvent = new CloudEventTopic()
                {
                    Id = Guid.NewGuid().ToString(),
                    Subject = contentRequestEvent.ReplySubject,
                    Source = "VAA.EAPI.Microservices.CRM.Content",
                    Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
                    Data = itemsData,
                    Topic = contentRequestEvent.ReplyTopic
                };

                AsyncContext.Current.MapToCloudEvent(responseEvent);
                await _messageBus.PublishMessageAsync(responseEvent);
            }
        }
    }
}
