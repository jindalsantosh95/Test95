namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty
{
    public class PointsCurrencyConverterResponse
    {
        public IEnumerable<LoyaltyPointsEquivalentCurrency> PointsEquivalentCurrencyDetails { get; set; }
        public IEnumerable<PtcWisePointsEquivalentCurrencyDetails> PtcWisePointsEquivalentCurrencyDetails { get; set; }
    }
}