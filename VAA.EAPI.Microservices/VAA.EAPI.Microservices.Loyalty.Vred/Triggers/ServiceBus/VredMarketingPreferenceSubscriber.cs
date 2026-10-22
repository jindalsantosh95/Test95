using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Triggers.ServiceBus
{
    public class VredMarketingPreferenceSubscriber
    {
        private readonly IMediator _mediator;

        public VredMarketingPreferenceSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("VredMarketingPreferenceSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.ServiceBusTopicNameForVredMarketing, Constants.Application.VredMarketingPreferenceEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            var consentData = Json.Deserialize<VredConsentReceiptCommand>(Convert.ToString(cloudEvent.Data));
            await _mediator.Send(consentData);
        }
    }
}
