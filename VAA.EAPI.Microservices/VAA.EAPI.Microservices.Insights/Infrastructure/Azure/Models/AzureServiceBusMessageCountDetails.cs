namespace VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models
{
    public class AzureServiceBusMessageCountDetails
    {
        public long ActiveMessageCount { get; set; }
        public long DeadLetterMessageCount { get; set; }
    }
}
