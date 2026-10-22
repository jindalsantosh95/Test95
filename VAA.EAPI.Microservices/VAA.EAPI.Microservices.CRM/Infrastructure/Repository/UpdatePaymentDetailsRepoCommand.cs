using MediatR;
using System.Net.Http.Headers;
using VAA.EAPI.Microservices.CRM.Domain.CRMSchema;

namespace VAA.EAPI.Microservices.CRM.Infrastructure.Repository
{
    public class UpdatePaymentDetailsRepoCommand : IRequest<HttpResponseHeaders>
    {
        public PaymentEntity paymentEntity { get; set; }
    }
}
