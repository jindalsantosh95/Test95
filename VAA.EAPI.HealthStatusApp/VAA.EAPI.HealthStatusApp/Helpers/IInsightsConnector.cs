using VAA.EAPI.HealthStatusApp.Enums;

namespace VAA.EAPI.HealthStatusApp.Helpers
{
    public interface IInsightsConnector
    {
        public Task<string> SendAsync(string route, InsightsEnvironment env = InsightsEnvironment.Default);
        public Task<string> SendAsync(string route, string body, InsightsEnvironment env = InsightsEnvironment.Default);
        public Task<(int Status, string Message)> GetInsightsStatus();
    }

}