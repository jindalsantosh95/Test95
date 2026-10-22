using VAA.EAPI.HealthStatusApp.Models.NewFolder;

namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class ServiceNowViewModel
    {
        public string IncidentNumber { get; set; }
        public string Category { get; set; }
        public string ConfigurationItem { get; set; }
        public string State { get; set; }
        public string Priority { get; set; }
        public string Channel { get; set; }
        public string Shortdescription { get; set; }
        public DateTime Created { get; set; }
    }
}
