namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.PaymentUpdate
{
    public class PaymentUpdateEventData
    {
        public string OrderReference { get; set; }
        public string Status { get; set; }
        public string PayLoad { get; set; }
    }
}
