namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS
{
    public class PaymentInfo
    {
        public string PaymentAmount { get; set; }
        public string DecimalPrecisionCnt { get; set; }
        public string CurrencyCode { get; set; }
        public bool PayWithMilesToggle { get; set; }
    }
}
