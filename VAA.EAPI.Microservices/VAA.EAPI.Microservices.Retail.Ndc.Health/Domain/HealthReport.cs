namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Domain
{
    public class HealthReport
    {
        public string API { get; set; }
        public IEnumerable<StatusInfo> StatusInfo { get; set; }
        public string TotalCount { get; set; }
    }
}
