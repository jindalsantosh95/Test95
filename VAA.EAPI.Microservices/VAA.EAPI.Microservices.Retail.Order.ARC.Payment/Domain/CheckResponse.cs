namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain
{
    public class CheckResponse
    {
        public TransactionMetadata TransactionMetadata { get; set; }
        public TransactionResponse TransactionResponse { get; set; }
        public string Message { get; set; }

    }
}
