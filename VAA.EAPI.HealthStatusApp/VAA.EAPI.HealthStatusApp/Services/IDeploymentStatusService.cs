namespace VAA.EAPI.HealthStatusApp.Services
{
    public interface IDeploymentStatusService
    {
        Task<Models.ADOPipelines.ADODeploymentCollection> GetDeploymentStatusesAsync();
    }
}
