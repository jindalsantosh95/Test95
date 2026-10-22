using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public class FcPointsPerTier
    {
        public IEnumerable<TierEarnPoints> Tiers { get; set; }
    }
}
