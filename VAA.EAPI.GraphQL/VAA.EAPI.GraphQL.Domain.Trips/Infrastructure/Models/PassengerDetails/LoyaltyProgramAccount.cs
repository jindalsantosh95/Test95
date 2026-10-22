namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    public class LoyaltyProgramAccount
    {
        public LoyaltyProgram LoyaltyProgram { get; set; }
        public string Number { get; set; }
        public string TierLevelCode { get; set; }
        public string TierLevelName { get; set; }
        public string TierFullLevelName { get; set; }
        public string TierLevelPriority { get; set; }
        public bool ThreeSixtyMember { get; set; }
    }
}