using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredStatements;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query
{
    public class VredStatementsQuery : IRequest<Result<Response<VredStatementResponse>>>
    {
        public string MemberNumber { get; set; }
        public VredStatementsQuery(string memberNumber)
        {
            MemberNumber = memberNumber;
        }
    }
}
