using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus
{
    internal class FCNumerToPNRResponseEventSubscriber
    {
        private readonly IMediator _mediator;

        public FCNumerToPNRResponseEventSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [Function("FCNumerToPNRResponseEventSubscriber")]
        public async Task GenerateFCToPNRResponseFile([ServiceBusTrigger(Constants.Application.ServiceBusTopicName, Constants.Application.FCNumberToPNRResponseEventSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            await _mediator.Send(Json.Deserialize<FCNumerToPNRResponseEventCommand>(eventRequestPayload));
        }
    }
}
