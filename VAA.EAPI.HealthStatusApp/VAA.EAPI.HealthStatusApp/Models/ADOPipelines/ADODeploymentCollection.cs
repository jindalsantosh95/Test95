namespace VAA.EAPI.HealthStatusApp.Models.ADOPipelines
{
    public class ADODeploymentCollection
    {
        public DateTime LastUpdated { get; set; }
        public IEnumerable<ADODeploymentStatus> DeploymentStatuses { get; set; } = Enumerable.Empty<ADODeploymentStatus>();
    }
}
