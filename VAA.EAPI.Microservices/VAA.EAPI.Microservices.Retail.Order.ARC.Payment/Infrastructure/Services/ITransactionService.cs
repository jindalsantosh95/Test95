using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Services
{
    public interface ITransactionService
    {
        Task<TransactionServiceResponse?> Handle(TransactionServiceRequest transactionRequest);
    }
}
