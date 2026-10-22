using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.FlightOrderNotification.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.ServiceBus
{
    public class FlightOrderDataRequestSubscriber(IFlightOrderNotificationDetails mediator)
    {
        private readonly IFlightOrderNotificationDetails _flightOrderNotificationDetails = mediator ?? throw new ArgumentNullException(nameof(mediator));

        [Function("FlightOrderDataRequestSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.NotificationTopicName, Constants.Application.FlightOrderFetcherSubscriberName, Connection = "NotificationServiceBusConnectionString")] string eventRequestPayload, CancellationToken cancellationToken)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _flightOrderNotificationDetails.GetFlightOrderDetailAsync(cloudEvent, cancellationToken);
        }
    }
}
