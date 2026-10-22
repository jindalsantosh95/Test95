namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS
{
    public class MilesEquivalentPrice
    {
        public bool CashPlusMiles { get; set; }
        public int DiscountMileCount { get; set; }
        public string MileCount { get; set; }
    }
}