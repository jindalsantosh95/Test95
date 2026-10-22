namespace VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction
{
    public class UpdateTransactionResponse
    {
        public TransactionResponse SuccessResponse { get; set; }
        public TransactionErrorResponse ErrorResponse { get; set; }
    }
}
