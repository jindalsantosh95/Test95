using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Orchestration.Notifications.Application.Events;

namespace VAA.EAPI.Orchestration.Notifications.Triggers.ServiceBus
{
    public class AircraftSwapsSubscriber(IAircraftSwapsEventCommand aircraftSwapsDataEventCommand)
    {
        private readonly IAircraftSwapsEventCommand _aircraftSwapsDataEventCommand = aircraftSwapsDataEventCommand;

        [Function("AircraftSwapsSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Constants.AircraftSwapTopicName, Constants.Constants.AircraftSwapsSubscriptionName, Connection = Constants.Environment.AIMSServiceBusConnectionString)] string eventRequestPayload, CancellationToken cancellationToken)
        {

            bool isAircraftSwapEnabled = ConfigReader.Try(Constants.Environment.AircraftSwapEnabled, false);
            if (!isAircraftSwapEnabled)
                return;

            var cloudEvent = new CloudEvent
            {
                Data = eventRequestPayload,
                Channel = ["AIMS"]
            };

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _aircraftSwapsDataEventCommand.Handle(cloudEvent, cancellationToken);
        }
    }
}
