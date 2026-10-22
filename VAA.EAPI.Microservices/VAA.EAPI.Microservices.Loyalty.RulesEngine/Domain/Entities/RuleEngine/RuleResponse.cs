using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine
{
    public class RuleResponse
    {
        public IEnumerable<Action> Actions { get; set; }
        public double BasePoints { get; set; }
        public double CabinBonusPoints { get; set; }
        public double TierBonusPoints { get; set; }
        public double PointsAward { get; set; }
        public double TierPointsAward { get; set; }
        public double MilesAward { get; set; }
    }

}
