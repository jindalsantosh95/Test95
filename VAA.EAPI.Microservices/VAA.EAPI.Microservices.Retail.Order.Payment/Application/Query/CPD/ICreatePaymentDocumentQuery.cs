using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Query.CPD
{
    public interface ICreatePaymentDocumentQuery
    {
        Task<Result<Response<PaymentResponse>>> Handle(string CorrelationId, string Type, CancellationToken cancellationToken);
    }
}
