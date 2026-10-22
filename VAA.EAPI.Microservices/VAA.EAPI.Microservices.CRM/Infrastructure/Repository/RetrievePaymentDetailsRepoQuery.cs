using MediatR;
using VAA.EAPI.Microservices.CRM.Domain;

namespace VAA.EAPI.Microservices.CRM.Infrastructure.Repository
{
    public class RetrievePaymentDetailsRepoQuery : IRequest<PaymentDetails>
    {
        public string PaymentNumber { get; set; }

        public RetrievePaymentDetailsRepoQuery(string paymentNumber)
        {
            PaymentNumber = paymentNumber;
        }
    }
}
