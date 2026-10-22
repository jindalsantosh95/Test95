using System.Linq;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.EarnCalculator
{
    public class SegmentResponse
    {
        public string AirlineCodeMarketing { get; set; }

        public string AirlineCodeOperating { get; set; }

        public string SegmentClass { get; set; }

        public string FlightNumberOperating { get; set; }

        public string FlightDate { get; set; }

        public string Origin { get; set; }

        public string Destination { get; set; }

        public string ProcessingComments { get; set; }

        public int PointsAward { get; set; }

        public int BasePoints { get; set; }

        public int CabinBonusPoints { get; set; }

        public int TierBonusPoints { get; set; }

        public int TierPointsAward { get; set; }

        public static explicit operator SegmentResponse(LoyaltyRuleEntityStateResponse resEntity) => new()
        {
            PointsAward = (int)resEntity.Customers.FirstOrDefault().RuleResponse.PointsAward,
            TierPointsAward = (int)resEntity.Customers.FirstOrDefault().RuleResponse.TierPointsAward,
            CabinBonusPoints = (int)resEntity.Customers.FirstOrDefault().RuleResponse.CabinBonusPoints,
            TierBonusPoints = (int)resEntity.Customers.FirstOrDefault().RuleResponse.TierBonusPoints,
            BasePoints = (int)resEntity.Customers.FirstOrDefault().RuleResponse.BasePoints,
            Origin = resEntity.Customers.FirstOrDefault().Origin,
            Destination = resEntity.Customers.FirstOrDefault().Destination,
            AirlineCodeOperating = resEntity.AirlineCodeOperating,
            FlightNumberOperating = resEntity.FlightNumberOperating,
            FlightDate = resEntity.FlightDate,
            SegmentClass = resEntity.Customers.FirstOrDefault().FlownClass,
            AirlineCodeMarketing = resEntity.Customers.FirstOrDefault().AirlineCodeMarketing,
            ProcessingComments = resEntity.Customers?.FirstOrDefault().InvalidRuleLog?.FirstOrDefault()?.Message ?? string.Empty
        };

    }
}
