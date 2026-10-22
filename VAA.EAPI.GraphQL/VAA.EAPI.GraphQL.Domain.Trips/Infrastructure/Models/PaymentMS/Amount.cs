namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS
{
    public class Amount
    {
        public string MonetaryAmount { get; set; }
        public int DecimalPrecisionCount { get; set; }
        public string Currency { get; set; }
    }
}