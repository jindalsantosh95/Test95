using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.CRM.Domain;

namespace VAA.EAPI.Microservices.CRM.Application.Query
{
    public class PaymentUpdateStatusResponseQuery : IRequest<Result<Response<PaymentResponse>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }
}
