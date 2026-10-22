using VAA.EAPI.HealthStatusApp.Models.ViewModels;

namespace VAA.EAPI.HealthStatusApp.Factories
{
    public interface IStatusTableFactory
    {
        public Task<StatusTables> GetAPIMStatusTable();
        public Task<StatusTables> GetLinkedServiceTable();
    }
}

