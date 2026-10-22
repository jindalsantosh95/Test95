using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredCardDetails;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository
{
    public class VredCardDetailsRepoQuery : IRequest<VredCardDetailsResponse>
    {
        public string MemberNumber { get; set; }

        public VredCardDetailsRepoQuery(string memberNumber)
        {
            MemberNumber = memberNumber;
        }
    }
}
