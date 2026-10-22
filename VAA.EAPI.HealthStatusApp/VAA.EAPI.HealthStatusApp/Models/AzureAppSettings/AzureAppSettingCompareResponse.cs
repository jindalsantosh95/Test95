namespace VAA.EAPI.HealthStatusApp.Models.AzureAppSettings
{
    public class AzureAppSettingCompareResponse
    {
        public DateTime SnapshotDateTime { get; set; }
        public List<AzureAppSettingsState> AppSettingsState { get; set; } = new List<AzureAppSettingsState>();
    }
}
