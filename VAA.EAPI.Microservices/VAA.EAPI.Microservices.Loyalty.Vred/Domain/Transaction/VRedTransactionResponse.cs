using VAA.EAPI.Microservices.Loyalty.Vred.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction
{
    public class VRedTransactionResponse 
    {
        public TransactionResponse SuccessResponse { get; set; }
        public VRedErrorResponse ErrorResponse { get; set; }
    }
}