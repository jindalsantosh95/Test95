using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.TransactionStatement;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class TransactionStatementTextQuery : IRequest<Result<Response<StatementTextResponse>>>
    {
        public StatementTextRequest StatementDescriptionRequest { get; set; }
    }
}
