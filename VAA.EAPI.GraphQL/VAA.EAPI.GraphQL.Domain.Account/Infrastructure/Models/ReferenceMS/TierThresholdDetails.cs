namespace VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS
{
    [RefDataType("thresholdValues", "thresholdValues")]
    public class TierThresholdDetails
    {
        public int TierPointsForRetainToGold { get; set; }
        public int TierPointsForRetainToSilver { get; set; }
        public int TierPointsForRedToSilver { get; set; }
        public int TierPointsForSilverToGold { get; set; }
    }
}
