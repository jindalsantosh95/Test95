using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS
{
    public sealed class GovernmentForm
    {
        public GovernmentFormsStatus Status { get; set; }
        public string? Id { get; set; }
        public string? LinkId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public DateTime? AvailableFromUtc { get; set; }
        public string? AvailableFromDescription { get; set; }
        public bool IsAvailable { get; set; }
    }
}
