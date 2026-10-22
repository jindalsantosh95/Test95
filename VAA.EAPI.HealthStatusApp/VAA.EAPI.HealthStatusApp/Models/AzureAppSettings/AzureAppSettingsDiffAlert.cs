namespace VAA.EAPI.HealthStatusApp.Models.AzureAppSettings;

public class AzureAppSettingsDiffAlert
{
    public DateTime Timestamp { get; set; }
    public string Subscription { get; set; } = string.Empty;
    public string ResourceGroup { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
}
