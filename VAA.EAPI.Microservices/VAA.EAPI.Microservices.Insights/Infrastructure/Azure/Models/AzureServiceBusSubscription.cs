namespace VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models
{
    public class AzureServiceBusSubscription
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public AzureServiceBusEntityProperties Properties { get; set; }

        public long ActiveQueueDepth => Properties?.CountDetails?.ActiveMessageCount ?? 0;

        public long DeadLetterQueueDepth => Properties?.CountDetails?.DeadLetterMessageCount ?? 0;
    }
}
