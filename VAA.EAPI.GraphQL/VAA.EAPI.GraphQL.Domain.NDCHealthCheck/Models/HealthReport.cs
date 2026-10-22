namespace VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Models
{
    public class HealthReport
    {
        public string API { get; set; }
        public IEnumerable<StatusInfo> StatusInfo { get; set; }
        public string TotalCount { get; set; }
    }
}
