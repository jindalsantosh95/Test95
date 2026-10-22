using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CPD
{
    public class GetTransactionStatusQuery : IRequest<Result<Response<GetTransactionStatusResponse>>>
    {
        public string TransactionStatusXmlRequest { get; set; }

    }
}
