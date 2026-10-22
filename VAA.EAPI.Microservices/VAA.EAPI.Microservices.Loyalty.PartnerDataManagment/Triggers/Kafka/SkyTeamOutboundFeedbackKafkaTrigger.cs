using Microsoft.Azure.Functions.Worker;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Triggers.Kafka
{
    public sealed class SkyTeamOutboundFeedbackKafkaTrigger(
        AutoAccrualFeedbackOutboundKafkaMessageHandler messageHandler)
    {
        [Function(nameof(SkyTeamOutboundFeedbackKafkaTrigger))]
        public Task HandleAsync(
            [KafkaTrigger(
                "%SkyTeamKafkaBootstrapServers%",
                "%AutoAccrualFeedbackOutboundKafkaTopic%",
                ConsumerGroup = "%AutoAccrualFeedbackOutboundKafkaConsumerGroup%")] string messageEnvelope,
            CancellationToken cancellationToken) =>
            messageHandler.HandleAsync(messageEnvelope, cancellationToken);
    }
}
