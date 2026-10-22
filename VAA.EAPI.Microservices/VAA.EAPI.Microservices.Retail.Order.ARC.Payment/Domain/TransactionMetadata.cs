namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain
{
    public class TransactionMetadata
    {
        public string CorrelationId { get; set; }
        public string Timestamp { get; set; }
        public string Message { get; set; }
    }
}
