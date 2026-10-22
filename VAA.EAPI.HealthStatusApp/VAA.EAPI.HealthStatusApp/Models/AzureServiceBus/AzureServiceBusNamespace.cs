namespace VAA.EAPI.HealthStatusApp.Models.AzureServiceBus;

public class AzureServiceBusNamespace
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public IEnumerable<AzureServiceBusTopic> Topics { get; set; } = [];
}
