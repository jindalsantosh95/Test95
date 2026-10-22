namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Payment
{
    public class Amount
    {
        public string MonetaryAmount { get; set; }
        public int DecimalPrecisionCount { get; set; }
        public string Currency { get; set; }
    }
}
