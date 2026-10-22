namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public sealed class FlyingClubAccountSummary
    {
        public string Tier { get; set; }
        public long VirginPoints { get; set; }
        public int TierPoints { get; set; }
        public DateTime? TierPointsResetDate { get; set; }
    }
}
