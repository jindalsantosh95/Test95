namespace VAA.EAPI.HealthStatusApp.Models.AzureServiceBus;

public class AzureServiceBusSubscription
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public AzureServiceBusEntityProperties? Properties { get; set; }

    public long ActiveQueueDepth => Properties?.CountDetails?.ActiveMessageCount ?? 0;

    public long DeadLetterQueueDepth => Properties?.CountDetails?.DeadLetterMessageCount ?? 0;
}
