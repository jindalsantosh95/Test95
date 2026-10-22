using VAA.EAPI.HealthStatusApp.Models.ViewModels;
using VAA.EAPI.HealthStatusApp.Services;

namespace VAA.EAPI.HealthStatusApp.Factories
{
    public class StatusTableFactory : IStatusTableFactory
    {
        private readonly IInsightsService _service;

        public StatusTableFactory(IInsightsService service)
        {
            _service = service;
        }

        public async Task<StatusTables> GetAPIMStatusTable()
        {
            try
            {
                var model = new StatusTables();
                model.APIMTable = await _service.GetAPIMStatusTableData();

                return model;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public async Task<StatusTables> GetLinkedServiceTable()
        {
            try
            {
                var model = new StatusTables();
                model.LinkedServicesTable = await _service.GetLinkedServiceTableData();

                return model;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}

