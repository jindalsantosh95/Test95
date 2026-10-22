namespace VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Models
{
    public class HealthCheckResponse
    {
        public string Api {  get; set; }
        public RAGStatus Status {  get; set; }
        public IEnumerable<Remarks> Remarks {  get; set; }
    }
}
