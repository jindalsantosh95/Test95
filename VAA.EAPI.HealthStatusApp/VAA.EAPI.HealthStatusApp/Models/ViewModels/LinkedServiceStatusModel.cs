namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class LinkedServiceStatusModel
    {
        public string? ServiceName { get; set; }
        public string ServiceType { get; set; }
        public string? ParentServiceCheckName { get; set; }
        public int StatusCode { get; set; }
        public string Description { get; set; }
    }
}
