namespace VAA.EAPI.HealthStatusApp.Models.AzureAppSettings;

public class AzureAppSettingsState
{
    public string Name { get; set; } = string.Empty;
    public string CurrentValue { get; set; } = string.Empty;
    public string? PreviousValue { get; set; }
}
