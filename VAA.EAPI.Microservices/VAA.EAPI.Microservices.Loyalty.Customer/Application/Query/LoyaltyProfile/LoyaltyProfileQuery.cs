using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.LoyaltyProfile
{
    public class LoyaltyProfileQuery : IRequest<Result<Response<CustomerProfile>>>
    {
        public string MemberNumber { get; set; }
        public LoyaltyProfileQuery(string memberNumber)
        {
            MemberNumber = memberNumber;
        }
    }
}