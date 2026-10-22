using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Transaction
{
    public class UpdateVredTransactionService
    {
        public UpdateVredTransactionRequest UpdateVredTransactionRequest { get; set; }

        public UpdateVredTransactionService(UpdateVredTransactionRequest updateVredTransactionRequest)
        {
            UpdateVredTransactionRequest = updateVredTransactionRequest;
        }
    }
}
