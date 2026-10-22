using VAA.EAPI.HealthStatusApp.Models.ViewModels;

namespace VAA.EAPI.HealthStatusApp.Factories
{
    public interface IDashboardFactory
    {
        public Task<DashboardModel> GetDashboadStatus();
        public Task<HomeModel> GetHomeDashboadStatus();
        public Task<HomeModel> GetEnvDashboadStatus(int env);
    }
}
