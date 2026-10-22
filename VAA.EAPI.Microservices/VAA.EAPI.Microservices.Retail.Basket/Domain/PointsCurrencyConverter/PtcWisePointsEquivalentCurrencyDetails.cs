using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain.PointsCurrencyConverter
{
    public class PtcWisePointsEquivalentCurrencyDetails
    {
        public IEnumerable<LoyaltyPointsEquivalentCurrency> PtcWisePointsEquivalentCurrency { get; set; }
        public LoyaltyPointsEquivalentCurrency TotalPointsEquivalentCurrency { get; set; }
    }
}