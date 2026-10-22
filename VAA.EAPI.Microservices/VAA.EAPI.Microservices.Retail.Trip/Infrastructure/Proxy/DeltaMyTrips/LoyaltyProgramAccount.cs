namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class LoyaltyProgramAccount
    {
        public LoyaltyProgram? LoyaltyProgram { get; set; }
        public string? Number { get; set; }
        public string? TierLevelCode { get; set; }
        public string? TierLevelName { get; set; }
        public bool ThreeSixtyMember { get; set; }
    }
}