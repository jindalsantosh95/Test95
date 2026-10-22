namespace VAA.EAPI.HealthStatusApp.Models.AzureServiceBus;

public class AzureServiceBusMessageCountDetails
{
    public long ActiveMessageCount { get; set; }
    public long DeadLetterMessageCount { get; set; }
}
