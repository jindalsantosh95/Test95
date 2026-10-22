namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS
{
    public class BaggagePrice
    {
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public decimal AdditionalBagPrice { get; set; }
        public int DecimalPrecisionCount { get; set; }
    }
}
