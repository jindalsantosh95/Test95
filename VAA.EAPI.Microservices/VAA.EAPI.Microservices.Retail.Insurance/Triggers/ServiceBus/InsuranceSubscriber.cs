using MediatR;
using Microsoft.Azure.Functions.Worker;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Insurance.Application.Commands;

namespace VAA.EAPI.Microservices.Retail.Insurance.Triggers.ServiceBus
{
    public class InsuranceSubscriber
    {
        private readonly IMediator _mediator;
        
        public InsuranceSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("InsuranceDecisionSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.InsuranceEventTopicName, Constants.Application.InsuranceDecisionEventSubscriberName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var decCommand = Json.Deserialize<DecisionCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(decCommand);

            await _mediator.Send(decCommand);
        }
    }
}
