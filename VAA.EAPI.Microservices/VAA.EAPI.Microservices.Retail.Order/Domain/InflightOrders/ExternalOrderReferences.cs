namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders
{
    public class ExternalOrderReferences
    {
        public string OwnerAirline { get; set; }
        public string OwnerNonAirline { get; set; }
        public string ExternalOrderId { get; set; }
        public string ExternalOrderItemId { get; set; }
    }
}
