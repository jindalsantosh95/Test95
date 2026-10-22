using System.Reflection.Metadata.Ecma335;
using VAA.EAPI.HealthStatusApp.Models.ViewModels;
using VAA.EAPI.HealthStatusApp.Services;

namespace VAA.EAPI.HealthStatusApp.Factories
{
    public class MetricsTableFactory : IMetricsTableFactory
    {
        private readonly IMetricsService _service;

        public MetricsTableFactory(IMetricsService service)
        {
            _service = service;
        }

        public async Task<MetricsModel> GetMetricsTable()
        {
            var model = new MetricsModel();
            model.DotComModel = new DotComModel();
            model.RedisCacheMetricsModels = await _service.GetRedisCacheMetrics();
            model.ServiceBusMetricsModels = await _service.GetServiceBusMetrics();
            model.DotComModel.ServicePlanModels = await _service.GetServicePlanMetrics();
            model.DotComModel.CosmosDbModels = await _service.GetCosmosDbMetrics();

            return model;
        }

        public async Task<IEnumerable<DeploymentMetricsModel>> GetDeploymentMetrics()
        {
            var result =  await _service.GetDeploymentMetrics();

            return result
                .Select(x =>
                {
                    x.ServiceName = x.ServiceName.Split('-')[0].Split('(')[0];
                    return x;
                })
                .OrderByDescending(y => y.LastDeployment);
        }


        public async Task<AppInsightsMetricsModel> GetHeimdallData(string timespan)
        {
            return await _service.GetHeimdallDataAsync(timespan);
        }
    }
}