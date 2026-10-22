using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities.LoyaltyProfile;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.LoyaltyProfile
{
    public class LoyaltyProfileRepoQuery : IRequest<LoyaltyProfileDetails>
    {
        public string MemberNumber { get; set; }

        public LoyaltyProfileRepoQuery(string memberNumber)
        {
            MemberNumber = memberNumber;
        }
    }
}