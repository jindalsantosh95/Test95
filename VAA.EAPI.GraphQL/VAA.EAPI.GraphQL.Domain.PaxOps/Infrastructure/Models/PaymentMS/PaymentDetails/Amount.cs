namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.PaymentMS.PaymentDetails
{
    public class Amount
    {
        public string MonetaryAmount { get; set; }
        public int DecimalPrecisionCount { get; set; }
        public string BaseCurrency { get; set; }
    }
}
