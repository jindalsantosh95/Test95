using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.CustomerProfile;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus
{
    public class UpdatePhoneNumberMessageSubscriber
    {
        private readonly ILogger<UpdatePhoneNumberMessageSubscriber> _logger;
        private readonly IUpdatePhoneNumberCommand _handler;

        public UpdatePhoneNumberMessageSubscriber(ILogger<UpdatePhoneNumberMessageSubscriber> logger, IUpdatePhoneNumberCommand handler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        [Function("UpdatePhoneNumberMessageSubscriber")]
        public async Task ProcessMessageAsync([ServiceBusTrigger(Constants.Application.ServiceBusTopicName, Constants.Application.UpdatePhoneNumberMessageSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(eventRequestPayload);

            try
            {
                var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
                VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);

                var updatePhoneNumberNotificationEventRequest =
                    Json.Deserialize<UpdatePhoneNumberNotificationEventRequest>(
                        cloudEvent.Data.ToString());

                var result = await _handler.Handle(updatePhoneNumberNotificationEventRequest, cancellationToken);
                if (result.IsFailure)
                {
                    throw new InvalidOperationException($"UpdatePhoneNumberNotificationEvent failed: {result.Error}");
                }

                _logger.LogInformation(
                    "Successfully processed UpdatePhoneNumberNotificationEvent. CloudEventId: {CloudEventId}",
                    cloudEvent.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process UpdatePhoneNumberNotificationEvent.", eventRequestPayload);

                throw;
            }
        }

    }
}
