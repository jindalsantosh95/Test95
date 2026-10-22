namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class SelectedOfferItem
    {
        public string OfferItemId { get; set; }
        public OfferItemTotalAmt OfferItemTotalAmt { get; set; }
        public RetailItemMetaData RetailItemMetaData { get; set; }
    }
}
