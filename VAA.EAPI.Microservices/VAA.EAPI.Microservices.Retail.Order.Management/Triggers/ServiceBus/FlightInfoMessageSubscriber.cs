using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.AirportDetails;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Triggers.ServiceBus
{
    public class FlightInfoMessageSubscriber
    {
        private readonly IMediator _mediator;
        public FlightInfoMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("FlightInfoMessageSubscriber")]
        public async Task FlightInfoMessageSubscribe([ServiceBusTrigger(Constants.Application.FlightInformationEventHandlerTopicName, Constants.Application.FlightInformationEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext executionContext)
        {
            var cloudEvent = Json.Deserialize<AirportDetailsEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent, CancellationToken.None);
        }
    }

}
