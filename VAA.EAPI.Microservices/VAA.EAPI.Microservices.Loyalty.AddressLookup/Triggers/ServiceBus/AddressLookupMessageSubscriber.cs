using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Triggers.ServiceBus
{
    public class AddressLookupMessageSubscriber
    {
        private readonly IMediator _mediator;
        public AddressLookupMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("EnrolCustomerAddressLookupSubscriber")]

        public async Task Run([ServiceBusTrigger(Constants.Application.AddressLookupEventHandlerTopicName, Constants.Application.AddressLookupEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("EnrolCustomerAddressLookupSubscriber");
            log.LogInformation("AddressLookup Event received");
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(AddressLookupCommandRequestMapper.EventRequestMapper(cloudEvent));

        }
    }
}
