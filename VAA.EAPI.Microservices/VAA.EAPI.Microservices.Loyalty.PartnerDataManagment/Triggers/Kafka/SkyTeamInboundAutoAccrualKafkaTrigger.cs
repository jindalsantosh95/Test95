using Microsoft.Azure.Functions.Worker;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Triggers.Kafka
{
    public class SkyTeamInboundAutoAccrualKafkaTrigger(ISkyTeamInboundAutoAccrualEventHandler inboundAutoAccrualEventHandler)
    {
        [Function("SkyTeamInboundAutoAccrualKafkaTrigger")]
        public async Task Run([KafkaTrigger("%SkyTeamKafkaBootstrapServers%", "%AccrualInboundKafkaTopic%", ConsumerGroup = "%SkyTeamKafkaConsumerGroup%")] string messageEnvelope, FunctionContext context)
        {
            if (!await inboundAutoAccrualEventHandler.Handle(messageEnvelope, context?.CancellationToken ?? CancellationToken.None))
            {
                throw new InvalidOperationException("Failed to process SkyTeam inbound auto accrual message.");
            }
        }
    }
}
