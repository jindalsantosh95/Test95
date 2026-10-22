namespace VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability
{

    public sealed class RewardSeatCalendarResult
    {
        public IEnumerable<RewardSeatCalendarCandidate> Candidates { get; set; } = [];
        public string FareFamilyMappingVersion { get; set; } = string.Empty;
        public bool Complete { get; set; }
    }
}
