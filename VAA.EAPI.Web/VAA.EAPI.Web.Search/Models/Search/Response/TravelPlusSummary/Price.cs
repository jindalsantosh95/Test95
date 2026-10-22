namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary
{
    public class Price
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public decimal Tax { get; set; }
        public decimal? PriceDifference { get; set; }
        public string PriceDifferenceSign { get; set; }
        public decimal AmountIncludingTax { get; set; }
        public string AwardPoints { get; set; }
    }
}