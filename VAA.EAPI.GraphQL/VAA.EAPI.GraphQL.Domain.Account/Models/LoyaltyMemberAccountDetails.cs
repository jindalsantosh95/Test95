namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class LoyaltyMemberAccountDetails
    {
        public string CustomerId { get; set; }
        public DateTime? ByDate { get; set; }
        public string RetainPointTotal { get; set; }
        public string RollingPointTotal { get; set; }
        public string TotalVsflownMiles { get; set; }
        public DateTime? TierStartDate { get; set; }
        public DateTime? TierEndDate { get; set; }
        public string NumberofYearsAtGold { get; set; }
        public string TotalVSTierPointsInCurrentYear { get; set; }
        public string TotalVSTierPointsSinceBecomingGold { get; set; }
        public int TierPointsForRetainToGold { get; set; }
        public int TierPointsForRetainToSilver { get; set; }
        public int TierPointsForRedToSilver { get; set; }
        public int TierPointsForSilverToGold { get; set; }
        public DateTime? WingsStartDate { get; set; }
        public string TotalTierPointsSinceBecomingWings { get; set; }
        public string TotalVSTierPointsSinceBecomingWings { get; set; }
        public DateTime? GoldStartDate { get; set; }
        public string RetainCoreTierPoints { get; set; }
        public string RollingCoreTierPoints { get; set; }
    }
}
