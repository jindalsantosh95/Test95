using MediatR;
using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query.Transaction
{
    public class TransactionResponseQuery : IRequest<Result<Response<TransactionQueryResponse>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }
}