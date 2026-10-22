
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.PassengerRetrieval;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.ServiceBus
{
    public class PassengerRetrievalSubscriber(IPassengerRetrieval passengerRetrieval)
    {
        private readonly IPassengerRetrieval _passengerRetrieval = passengerRetrieval ?? throw new ArgumentNullException(nameof(passengerRetrieval));

        [Function("PassengerRetrievalSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Environment.FlightArrivalDetailsTopic, Constants.Environment.FlightArrivalDetailsSubscriber, Connection = Constants.Environment.FlightArrivalServiceBusConnectionString)] string eventRequestPayload, CancellationToken cancellationToken)
        {
            var cloudEvent = Json.Deserialize<EAPI.Domain.Events.CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _passengerRetrieval.GetPassengerFlightDetailAsync(cloudEvent, cancellationToken);

        }
    }
}