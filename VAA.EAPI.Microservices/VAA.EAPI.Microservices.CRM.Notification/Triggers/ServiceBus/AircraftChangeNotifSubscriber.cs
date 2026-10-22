using Microsoft.Azure.Functions.Worker;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.CRM.Notification.Application.Events;

namespace VAA.EAPI.Microservices.CRM.Notification.Triggers.ServiceBus
{
    public class AircraftChangeNotifSubscriber
    {
        private readonly IAircraftChangeNotifCommand _aircraftChangeNotifCommand;

        public AircraftChangeNotifSubscriber(IAircraftChangeNotifCommand aircraftChangeNotifCommand)
        {
            _aircraftChangeNotifCommand = aircraftChangeNotifCommand ?? throw new ArgumentNullException(nameof(aircraftChangeNotifCommand));
        }

        [Function("AircraftChangeNotifSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Constants.NotificationTopicName, Constants.Constants.AircraftChangeNotifSubscriberName, Connection = Constants.Environment.NotificationServiceBusConnectionString)] string eventRequestPayload, CancellationToken cancellationToken)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload); 

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _aircraftChangeNotifCommand.Handle(cloudEvent, cancellationToken);
        }
    }
}
