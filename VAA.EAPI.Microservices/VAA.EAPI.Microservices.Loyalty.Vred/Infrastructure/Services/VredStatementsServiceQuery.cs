using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredStatements;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services
{
    public class VredStatementsServiceQuery : IRequest<VredStatementResponse>
    {
        public string MemberNumber { get; set; }

        public VredStatementsServiceQuery(string memberNumber)
        {
            MemberNumber = memberNumber;
        }
    }
}
