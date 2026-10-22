using System.ServiceModel.Syndication;
using VAA.EAPI.HealthStatusApp.Models.NewFolder;

namespace VAA.EAPI.HealthStatusApp.Services
{
    public interface IServiceNowService
    {
        public Task<IEnumerable<ServiceNowModel>> GetServiceNowFeedAsync();
    }
}
