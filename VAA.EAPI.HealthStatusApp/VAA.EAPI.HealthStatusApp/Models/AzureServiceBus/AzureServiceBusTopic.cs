namespace VAA.EAPI.HealthStatusApp.Models.AzureServiceBus;

public class AzureServiceBusTopic
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public AzureServiceBusEntityProperties? Properties { get; set; }
    public IEnumerable<AzureServiceBusSubscription> Subscriptions { get; set; } = [];

    public long ActiveMessageCount => Properties?.CountDetails?.ActiveMessageCount ?? 0;

    public long DeadLetterMessageCount => Properties?.CountDetails?.DeadLetterMessageCount ?? 0;
}
