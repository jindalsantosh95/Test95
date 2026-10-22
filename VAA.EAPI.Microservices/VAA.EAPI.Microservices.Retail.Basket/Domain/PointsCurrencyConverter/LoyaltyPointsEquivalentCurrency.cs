using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain.PointsCurrencyConverter
{
    public class LoyaltyPointsEquivalentCurrency
    {
        public CurrencyAmount Amount { get; set; }
        public string LoyaltyPoints { get; set; }
        public string PassengerTypeCode { get; set; }
    }
}