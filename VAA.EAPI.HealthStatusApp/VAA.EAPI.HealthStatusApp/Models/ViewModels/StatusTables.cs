namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class StatusTables
    {
        public IEnumerable<APIMStatusModel> APIMTable { get; set; }
        public IEnumerable<LinkedServiceStatusModel> LinkedServicesTable { get; set; }
    }
}
