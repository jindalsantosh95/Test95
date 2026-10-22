namespace VAA.EAPI.GraphQL.Domain.Book.Models.Payment
{
    public class MilesEquivalentPrice
    {
        public bool CashPlusMiles { get; set; }
        public int DiscountMileCount { get; set; }
        public string MileCount { get; set; }
    }
}