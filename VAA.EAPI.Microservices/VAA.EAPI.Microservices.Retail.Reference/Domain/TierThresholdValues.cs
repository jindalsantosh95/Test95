using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class TierThresholdValues
    {
        public int TierPointsForRetainToGold { get; set; }

        public int TierPointsForRetainToSilver { get; set; }

        public int TierPointsForRedToSilver { get; set; }

        public int TierPointsForSilverToGold { get; set; }

        public static explicit operator TierThresholdValues(MemberAccountReferenceData refData) => new()
        {
            TierPointsForRetainToGold = refData.TierPointsForRetainToGold,
            TierPointsForRetainToSilver = refData.TierPointsForRetainToSilver,
            TierPointsForRedToSilver = refData.TierPointsForRedToSilver,
            TierPointsForSilverToGold = refData.TierPointsForSilverToGold,
        };
    }
}
