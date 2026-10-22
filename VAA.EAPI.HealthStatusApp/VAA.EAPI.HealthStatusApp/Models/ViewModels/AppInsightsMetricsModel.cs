using VAA.EAPI.HealthStatusApp.Models.MetricsModels;

namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class AppInsightsMetricsModel
    {
        public LatestHeimdallResult LatestHeimdallResult { get; set; }
        public int TotalSuccessful { get; set; }
        public int TotalFailed { get; set; }
        public IEnumerable<HeimdallException> Exceptions { get; set; }

    }
}
