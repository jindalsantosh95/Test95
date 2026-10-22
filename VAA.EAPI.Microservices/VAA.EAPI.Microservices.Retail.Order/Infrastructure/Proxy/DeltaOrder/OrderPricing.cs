namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class OrderPricing
    {
        public TotalAmt TotalAmt { get; set; }
        public BaseAmt BaseAmt { get; set; }
        public TaxSummary TaxSummary { get; set; }
        public OriginalTotalAmt OriginalTotalAmt { get; set; }
    }
}
