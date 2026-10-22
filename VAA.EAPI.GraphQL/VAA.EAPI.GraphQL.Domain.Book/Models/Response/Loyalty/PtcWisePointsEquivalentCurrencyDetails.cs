namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty
{
    public class PtcWisePointsEquivalentCurrencyDetails
    {
        public IEnumerable<LoyaltyPointsEquivalentCurrency> PtcWisePointsEquivalentCurrency { get; set; }
        public LoyaltyPointsEquivalentCurrency TotalPointsEquivalentCurrency { get; set; }
    }
}