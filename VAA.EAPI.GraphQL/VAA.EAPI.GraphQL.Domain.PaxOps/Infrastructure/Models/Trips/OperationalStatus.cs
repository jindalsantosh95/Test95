namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips
{
    public class OperationalStatus
    {
        public string? Type { get; set; }
        public string? SubType { get; set; }
        public bool? RebookEligible { get; set; }
    }
}