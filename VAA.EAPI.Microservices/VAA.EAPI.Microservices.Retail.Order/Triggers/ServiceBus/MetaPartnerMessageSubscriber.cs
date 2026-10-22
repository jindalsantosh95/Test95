using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.MetaPartners;

namespace VAA.EAPI.Microservices.Retail.Order.Triggers.ServiceBus
{
    public class MetaPartnerMessageSubscriber(ServiceBusClient serviceBusClient, IMetaPartnerCommandHandler metaPartnerCommandHandler, ILogger<MetaPartnerMessageSubscriber> logger)
    {
        private const int MaxRetries = 30;

        private readonly ServiceBusClient _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
        private readonly IMetaPartnerCommandHandler _metaPartnerCommandHandler = metaPartnerCommandHandler ?? throw new ArgumentNullException(nameof(metaPartnerCommandHandler));
        private readonly ILogger<MetaPartnerMessageSubscriber> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [Function("MetaPartnerMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.MetaPartnerEventHandlerTopicName, Constants.Application.MetaPartnerEventSubscriptionName, Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, FunctionContext context)
        {
            var retryCount = message.ApplicationProperties.ContainsKey("RetryCount") ? (int)message.ApplicationProperties["RetryCount"] : 0;

            try
            {
                var cloudEvent = message.Body.ToString();

                var metaPartnerEventCommand = Json.Deserialize<MetaPartnerEventCommand>(cloudEvent);

                AsyncContext.Current.MapFromCloudEvent(metaPartnerEventCommand);

                await _metaPartnerCommandHandler.Handle((MetaPartnerCommand)metaPartnerEventCommand, CancellationToken.None);

                await messageActions.CompleteMessageAsync(message);
            }
            catch (Exception ex)
            {
                /*
                First retry: after (approximately) 1 minute, from the time of the original attempt
                Second retry: after (approximately) 30 minutes, from the time of the original attempt
                Third retry: after (approximately) 24 hours from the time of the original attempt

                If, after all three retries, there has still not been a successful response, you must retain the event for a further 30 days, with the ability to manually retry within that time period.
                You should log the responses of each failed attempt, to help in any resolution efforts.
                You must notify Skyscanner Partner Services when an event fails on all three retries.

                */
                _logger.LogError(ex, $"An exception has occurred processing message '{message.MessageId}'");

                if (retryCount >= MaxRetries)
                {
                    logger.LogWarning("Message ID: {id} exhausted max retries. Dead-lettering.", message.MessageId);

                    await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "Max retries exceeded");
                }
                else
                {
                    retryCount++;

                    var sender = _serviceBusClient.CreateSender(Constants.Application.MetaPartnerEventHandlerTopicName);
                    var clonedMessage = new ServiceBusMessage(message).AddEapiTraceContext();

                    clonedMessage.ApplicationProperties["RetryCount"] = retryCount;

                    double delayInSeconds = 0D;

                    switch (retryCount)
                    {
                        case 1:

                            delayInSeconds = TimeSpan.FromMinutes(1).TotalSeconds;

                            break;
                        case 2:

                            delayInSeconds = TimeSpan.FromMinutes(30).TotalSeconds;

                            break;
                        default:
                            
                            delayInSeconds = TimeSpan.FromDays(1).TotalSeconds;

                            break;
                    }

                    logger.LogInformation("Scheduling retry for message ID: {id} in {delay} seconds. Retry count: {retryCount}", message.MessageId, delayInSeconds, retryCount);

                    await sender.ScheduleMessageAsync(clonedMessage, DateTimeOffset.UtcNow.AddSeconds(delayInSeconds));

                    // Complete the original message to remove it from the queue
                    await messageActions.CompleteMessageAsync(message);
                }
            }
        }

    }
}
