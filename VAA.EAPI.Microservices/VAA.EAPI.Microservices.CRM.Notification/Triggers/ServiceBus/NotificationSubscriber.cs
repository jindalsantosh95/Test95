using Microsoft.Azure.Functions.Worker;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.CRM.Notification.Application.Events;

namespace VAA.EAPI.Microservices.CRM.Notification.Triggers.ServiceBus
{
    public class NotificationSubscriber
    {
        private readonly INotificationEventCommand _notificationEventCommand;

        public NotificationSubscriber(INotificationEventCommand notificationEventCommand)
        {
            _notificationEventCommand = notificationEventCommand ?? throw new ArgumentNullException(nameof(notificationEventCommand)); 
        }

        [Function("NotificationSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Constants.NotificationTopicName, Constants.Constants.NotificationSubscriberName, Connection = Constants.Environment.NotificationServiceBusConnectionString)] string eventRequestPayload, CancellationToken cancellationToken)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);

            AsyncContext.Current.SetDiagnosticsEnabled(cloudEvent.DiagnosticsEnabled);
            AsyncContext.Current.SetSessionId(cloudEvent.SessionId);
            AsyncContext.Current.SetOperationId(cloudEvent.OperationId);
            AsyncContext.Current.SetCorrelationId(cloudEvent.CorrelationId);
            AsyncContext.Current.SetChannel(cloudEvent.Channel);

            await _notificationEventCommand.Handle(cloudEvent, cancellationToken);
        }
    }
}
