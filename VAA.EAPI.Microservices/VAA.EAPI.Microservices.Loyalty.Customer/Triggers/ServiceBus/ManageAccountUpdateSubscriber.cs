using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.ServiceBus
{
    public class ManageAccountUpdateSubscriber
    {
        private readonly IMediator _mediator;
        public ManageAccountUpdateSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("AddressUpdateSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.ManagaeAccountTopicName, Constants.Application.UpdateAddressMASubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(Convert.ToString(eventRequestPayload));
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(Json.Deserialize<UpdateMemberAddressCommand>(Convert.ToString(cloudEvent.Data)));
        }

        [Function("UpdatePhoneNumberSubscriber")]
        public async Task UpdatePhoneSubscriberAsync([ServiceBusTrigger(Constants.Application.ManagaeAccountTopicName, Constants.Application.UpdatePhoneNumberMASubsriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(Convert.ToString(eventRequestPayload));
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(Json.Deserialize<UpdatePhoneNumberCommand>(Convert.ToString(cloudEvent.Data)));
        }
    }
}
