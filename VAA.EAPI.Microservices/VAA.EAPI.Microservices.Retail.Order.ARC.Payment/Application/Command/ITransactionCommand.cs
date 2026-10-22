using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Application.Command
{
    public interface ITransactionCommand
    {
        Task<PaymentTransactionResponse> Handle(TransactionRequest request);
    }
}
