using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories
{
    public class PaymentDocumentReaderQuery : IRequest<PaymentTransactionDocument>
    {
        public string TransactionId { get; set; }
        public string SqlQuery { get; set; }
    }
}
