using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Triggers.ServiceBus
{
    public class SkyTeamInboundAutoAccrualProcessingSubscriber
    {
        private readonly ISkyTeamInboundAutoAccrualProcessingEventHandler _processingEventHandler;
        private readonly ILogger<SkyTeamInboundAutoAccrualProcessingSubscriber> _logger;

        public SkyTeamInboundAutoAccrualProcessingSubscriber(ISkyTeamInboundAutoAccrualProcessingEventHandler processingEventHandler, ILogger<SkyTeamInboundAutoAccrualProcessingSubscriber> logger)
        {
            _processingEventHandler = processingEventHandler ?? throw new ArgumentNullException(nameof(processingEventHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("SkyTeamInboundAutoAccrualProcessingSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.PdmEventTopicName, Constants.Application.SkyTeamInboundAutoAccrualProcessingSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var eventMessage = Json.Deserialize<AutoAccrualProcessingMessage>(eventRequestPayload) ?? throw new InvalidOperationException("SkyTeam auto accrual processing message deserialized to null.");
            AsyncContext.Current?.MapFromCloudEvent(eventMessage);

            if (eventMessage.Data is null)
            {
                throw new InvalidOperationException("SkyTeam auto accrual processing message is missing required 'data' payload.");
            }

            if (!await _processingEventHandler.Handle(eventMessage.Data, context?.CancellationToken ?? CancellationToken.None))
            {
                throw new InvalidOperationException("Failed to process SkyTeam inbound auto accrual request.");
            }
        }
    }
}
