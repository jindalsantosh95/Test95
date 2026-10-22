using Microsoft.Azure.Functions.Worker;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Orchestration.Notifications.Application.Events;

namespace VAA.EAPI.Orchestration.Notifications.Triggers.ServiceBus
{
    public class AcSwapsFlightDataRequestSubscriber(IAcSwapsFlightDataRequestEventCommand acSwapsFlightDataRequestEventCommand)
    {
        private readonly IAcSwapsFlightDataRequestEventCommand _acSwapsFlightDataRequestEventCommand = acSwapsFlightDataRequestEventCommand;

        [Function("AcSwapsFlightDataRequestSubscriber")]        
        public async Task Run([ServiceBusTrigger(Constants.Constants.NotificationTopicName, Constants.Constants.AcSwapsFlightDataRequestSubscriptionName, Connection = Constants.Environment.NotificationServiceBusConnectionString)] string eventRequestPayload, CancellationToken cancellationToken)
        {            
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);
           
            await _acSwapsFlightDataRequestEventCommand.Handle(cloudEvent, cancellationToken);
        }
    }
}
