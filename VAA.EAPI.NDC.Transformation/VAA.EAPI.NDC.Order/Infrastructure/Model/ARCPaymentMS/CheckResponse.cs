namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS
{
    public class CheckResponse
    {
        public TransactionMetadata TransactionMetadata { get; set; }
        public TransactionResponse TransactionResponse { get; set; }
        public string Message { get; set; }
    }
}
