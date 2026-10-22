using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Loyalty
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
