using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Mapper
{
    public class CustomerRequestMapper
    {
        public static Customer RequestMapper(LoyaltySyncRequestData request)
        {
            return new Customer
            {
                CustId = request.CustomerId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
                LoyaltyMemberDetails = new LoyaltyMemberDetails() { MemberNumber = request.FcNumber }
            };
        }
    }
}
