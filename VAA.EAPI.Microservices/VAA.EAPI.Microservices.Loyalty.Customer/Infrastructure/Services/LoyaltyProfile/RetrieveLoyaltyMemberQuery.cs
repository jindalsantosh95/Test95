using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Services.LoyaltyProfile
{
    public class RetrieveLoyaltyMemberQuery : IRequest<KeyValuePair<bool, string>>
    {
        public string MemberNumber { get; set; }

        public RetrieveLoyaltyMemberQuery(string memberNumber)
        {
            MemberNumber = memberNumber;
        }
    }
}