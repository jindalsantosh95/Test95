using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities.PointsCurrencyConverter
{
    public class PointsCurrencyConverterResponse
    {
        public IEnumerable<LoyaltyPointsEquivalentCurrency> PointsEquivalentCurrencyDetails { get; set; }
        public IEnumerable<PtcWisePointsEquivalentCurrencyDetails> PtcWisePointsEquivalentCurrencyDetails { get; set; }
    }
}