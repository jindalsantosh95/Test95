using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities.PointsCurrencyConverter
{
    public class PointsCurrencyConverterRequest
    {
        public string CountryCode { get; set; }
        public string EligibilityIndicator { get; set; }
        public int NumberOfPassengers { get; set; }
        public string PassengerTypeCode { get; set; }
        public string Tier { get; set; }
        public string LoyaltyPoints { get; set; }
        public CurrencyAmount Amount { get; set; }
    }
}