using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.EmailUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus
{
    public class EmailChangeEventSubscriber
    {
        private readonly ILogger<EmailChangeEventSubscriber> _logger;
        private readonly IEmailChangeNotificationCommand _handler;
       
        public EmailChangeEventSubscriber(ILogger<EmailChangeEventSubscriber> logger, IEmailChangeNotificationCommand handler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        [Function("EmailChangeEventSubscriber")]
        public async Task ProcessMessageAsync([ServiceBusTrigger(Constants.Application.ServiceBusTopicName, Constants.Application.EmailChangeEventSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(eventRequestPayload);

            try
            {
                var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
                VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);

                var emailChangeEvent =
                    Json.Deserialize<EmailChangeEvent>(
                        cloudEvent.Data.ToString());

                await _handler.Handle(emailChangeEvent, cancellationToken);

                _logger.LogInformation(
                    "Successfully processed EmailChangeEvent.",
                    eventRequestPayload);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process EmailChangeEvent.",
                    eventRequestPayload);

                throw;
            }
        }
    }
}
