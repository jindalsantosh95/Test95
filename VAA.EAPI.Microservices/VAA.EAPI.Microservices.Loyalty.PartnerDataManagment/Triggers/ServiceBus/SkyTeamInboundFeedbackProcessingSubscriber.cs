using Microsoft.Azure.Functions.Worker;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Triggers.ServiceBus
{
    public class SkyTeamInboundFeedbackProcessingSubscriber
    {
        private readonly ISkyTeamInboundFeedbackProcessingEventHandler _eventHandler;

        public SkyTeamInboundFeedbackProcessingSubscriber(ISkyTeamInboundFeedbackProcessingEventHandler eventHandler)
        {
            _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
        }

        [Function("SkyTeamInboundFeedbackProcessingSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.PdmEventTopicName, Constants.Application.SkyTeamInboundFeedbackProcessingSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var eventMessage = Json.Deserialize<AutoAccrualFeedbackProcessingMessage>(eventRequestPayload) ?? throw new InvalidOperationException("Auto accrual feedback processing message deserialized to null.");
            AsyncContext.Current?.MapFromCloudEvent(eventMessage);

            if (eventMessage.Data is null || !await _eventHandler.Handle(eventMessage.Data, context.CancellationToken))
            {
                throw new InvalidOperationException("Failed to store auto accrual feedback processing outcome.");
            }
        }
    }
}
