using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredStatements;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository
{
    public class VredStatementsRepoQuery : IRequest<VredStatementResponse>
    {
        public string MemberNumber { get; set; }
        public VredStatementsRepoQuery(string memberNumber)
        {
            MemberNumber = memberNumber;
        }
    }

}
