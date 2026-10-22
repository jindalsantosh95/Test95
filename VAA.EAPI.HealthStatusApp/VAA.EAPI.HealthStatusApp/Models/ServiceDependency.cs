namespace VAA.EAPI.HealthStatusApp.Models
{
    public class ServiceDependency
    {
        public required string DependencyName { get; set; }
        public ServiceDependencyType ServiceDependencyType { get; set; }
        public bool IsHealthy { get; set; }
    }
}
