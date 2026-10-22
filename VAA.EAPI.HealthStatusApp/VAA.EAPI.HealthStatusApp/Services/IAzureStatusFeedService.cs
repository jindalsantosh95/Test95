using System.ServiceModel.Syndication;

namespace VAA.EAPI.HealthStatusApp.Services
{
    public interface IAzureStatusFeedService
    {
        public Task<IEnumerable<SyndicationItem>> GetRssFeedAsync();
        public Task<int> CalculateAzureScore();
    }
}
