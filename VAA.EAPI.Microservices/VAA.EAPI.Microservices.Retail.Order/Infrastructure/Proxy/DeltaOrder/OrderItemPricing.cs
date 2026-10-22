namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class OrderItemPricing
    {
        public TotalAmt TotalAmt { get; set; }
        public BaseAmt BaseAmt { get; set; }
        public TaxSummary TaxSummary { get; set; }
    }
}
