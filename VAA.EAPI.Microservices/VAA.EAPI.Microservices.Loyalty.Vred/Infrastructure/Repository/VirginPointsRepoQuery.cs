using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository
{
    public class VirginPointsRepoQuery : IRequest<VRedResponse>
    {
        public string MemberID { get; set; }

        public VirginPointsRepoQuery(string memberID)
        {
            MemberID = memberID;
        }
    }
}
