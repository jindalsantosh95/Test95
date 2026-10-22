namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class AnnualRevenueRecord
    {
        public int? Year { get; set; }
        public CurrencyAmount Revenue { get; set; }
        public LocalAndUTCDateTime DateFirstSaleInYear { get; set; }
    }
}
