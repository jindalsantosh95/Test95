namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class APIMStatusModel
    {
        public string? ServiceName { get; set; }
        public string ServiceUrl { get; set; }
        public string ServiceType { get; set; }
        public int StatusCode { get; set; }

        public int HealthyStatus { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, bool> ServiceBusChecks { get; set; }
        public Dictionary<string, bool> CosmosChecks { get; set; }

    }
}
