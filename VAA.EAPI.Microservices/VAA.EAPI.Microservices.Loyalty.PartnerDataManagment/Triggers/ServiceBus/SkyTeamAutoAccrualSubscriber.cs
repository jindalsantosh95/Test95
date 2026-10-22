using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Triggers.ServiceBus
{
    public class SkyTeamAutoAccrualSubscriber
    {
        private readonly ISkyTeamAutoAccrualEventHandler _skyTeamAutoAccrualEventHandler;
        private readonly ILogger<SkyTeamAutoAccrualSubscriber> _logger;

        public SkyTeamAutoAccrualSubscriber(ISkyTeamAutoAccrualEventHandler skyTeamAutoAccrualEventHandler, ILogger<SkyTeamAutoAccrualSubscriber> logger)
        {
            _skyTeamAutoAccrualEventHandler = skyTeamAutoAccrualEventHandler ?? throw new ArgumentNullException(nameof(skyTeamAutoAccrualEventHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("SkyTeamAutoAccrualSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.PdmEventTopicName, Constants.Application.SkyTeamAutoAccrualSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var eventMessage = Json.Deserialize<AutoAccrualMessage>(eventRequestPayload) ?? throw new InvalidOperationException("Auto accrual message payload deserialized to null.");
            if (eventMessage.Data is null)
            {
                _logger.LogError("Auto accrual message missing required 'data' payload. Payload: {Payload}", eventRequestPayload);
                throw new InvalidOperationException("Auto accrual message is missing required 'data' payload.");
            }

            var handled = await _skyTeamAutoAccrualEventHandler.Handle(eventMessage.Data, context.CancellationToken);

            if (!handled)
            {
                throw new InvalidOperationException("Failed to process SkyTeam auto accrual message.");
            }
        }
    }
}
