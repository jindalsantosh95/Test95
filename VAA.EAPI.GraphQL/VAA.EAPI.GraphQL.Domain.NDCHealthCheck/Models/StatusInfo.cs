namespace VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Models
{
    public class StatusInfo
    {
        public string StatusCode { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}
