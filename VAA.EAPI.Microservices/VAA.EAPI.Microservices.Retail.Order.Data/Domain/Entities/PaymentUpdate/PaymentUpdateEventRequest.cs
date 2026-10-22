namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.PaymentUpdate
{
    public class PaymentUpdateEventRequest
    {
        public string Type { get; set; }
        public string Source { get; set; }
        public string Subject { get; set; }
        public string Id { get; set; }
        public string Time { get; set; }
        public string Specversion { get; set; }
        public PaymentUpdateRequestData Data { get; set; }
    }
}
