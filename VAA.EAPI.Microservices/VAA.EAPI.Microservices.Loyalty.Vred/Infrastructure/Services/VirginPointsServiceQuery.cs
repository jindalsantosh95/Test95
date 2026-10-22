using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services
{
    public class VirginPointsServiceQuery : IRequest<VRedResponse>
    {
        public string MemberID { get; set; }

        public VirginPointsServiceQuery(string memberID)
        {
            MemberID = memberID;
        }
    }
}
