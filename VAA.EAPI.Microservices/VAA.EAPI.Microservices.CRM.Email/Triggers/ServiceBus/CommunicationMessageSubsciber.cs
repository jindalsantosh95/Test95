using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.CRM.Email.Application.Command.Communication;

namespace VAA.EAPI.Microservices.CRM.Email.Triggers.ServiceBus
{
    public class CommunicationMessageSubsciber
    {
        private readonly IMediator _mediator;
        public CommunicationMessageSubsciber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("CommunicationMessageSubsciber")]
        public async Task Run([ServiceBusTrigger("%PaymentCommunicationEventHandlerTopicName%", "%PaymentCommunicationEventSubscriptionName%", Connection = "SovServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("CommunicationMessageSubsciber");
            log.LogInformation("CommunicationMessageSubsciber received");
            var communicationMessageRequest = Json.Deserialize<CommunicationEventCommand>(eventRequestPayload);
            log.LogInformation($"CommunicationMessageSubsciber | Correlational ID :  {communicationMessageRequest.Id}");
            await _mediator.Send(communicationMessageRequest, CancellationToken.None);
        }
    }
}
