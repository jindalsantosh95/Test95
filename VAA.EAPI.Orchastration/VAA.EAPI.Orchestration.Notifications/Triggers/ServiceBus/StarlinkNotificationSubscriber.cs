using Microsoft.Azure.Functions.Worker;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Orchestration.Notifications.Application.Events;

namespace VAA.EAPI.Orchestration.Notifications.Triggers.ServiceBus
{
    public class StarlinkNotificationSubscriber(IStarlinkNotificationEventCommand starlinkNotificationEventCommand)
    {
        private readonly IStarlinkNotificationEventCommand _starlinkNotificationEventCommand = starlinkNotificationEventCommand;

        [Function("StarlinkNotificationSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Constants.NotificationTopicName, Constants.Constants.StarlinkEnabledFlightSubscriptionName, Connection = Constants.Environment.NotificationServiceBusConnectionString)] string eventRequestPayload, CancellationToken cancellationToken)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);

            AsyncContext.Current.SetDiagnosticsEnabled(cloudEvent.DiagnosticsEnabled);
            AsyncContext.Current.SetSessionId(cloudEvent.SessionId);
            AsyncContext.Current.SetOperationId(cloudEvent.OperationId);
            AsyncContext.Current.SetCorrelationId(cloudEvent.CorrelationId);
            AsyncContext.Current.SetChannel(cloudEvent.Channel);

            await _starlinkNotificationEventCommand.Handle(cloudEvent, cancellationToken);
        }
    }
}
