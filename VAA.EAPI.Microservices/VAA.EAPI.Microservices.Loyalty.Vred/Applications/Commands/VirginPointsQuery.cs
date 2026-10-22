using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands
{
    public class VirginPointsQuery : IRequest<VRedResponse>
    {
        public string MemberID { get; set; }
    }
}
