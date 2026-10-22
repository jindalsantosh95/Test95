using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredCardDetails;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services
{
    public class VredCardDetailsServiceQuery : IRequest<VredCardDetailsResponse>
    {
        public string MemberNumber { get; set; }

        public VredCardDetailsServiceQuery(string memberNumber)
        {
            MemberNumber = memberNumber;
        }
    }
}
