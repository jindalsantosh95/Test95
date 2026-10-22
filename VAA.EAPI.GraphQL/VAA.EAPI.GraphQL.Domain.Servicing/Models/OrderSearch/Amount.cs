namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class Amount
    {
        public string? MonetaryAmount { get; set; }
        public int DecimalPrecisionCnt { get; set; }
        public string? BaseCurrency { get; set; }
    }
}
