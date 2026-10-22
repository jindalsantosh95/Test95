using VAA.EAPI.GraphQL.Domain.Book.Models.Common;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty
{
    public class LoyaltyPointsEquivalentCurrency
    {
        public CurrencyAmount Amount { get; set; }
        public string LoyaltyPoints { get; set; }
        public string PassengerTypeCode { get; set; }
    }
}