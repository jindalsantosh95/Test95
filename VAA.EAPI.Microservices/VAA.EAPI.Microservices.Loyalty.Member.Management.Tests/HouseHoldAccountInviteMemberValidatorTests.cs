using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class HouseHoldAccountInviteMemberValidatorTests
    {
        [Fact]
        public void MapInviteMemberValidator_RejectsInactiveHouseholdLeader()
        {
            var response = HouseHoldAccountInviteMemberValidator.MapInviteMemberValidator(
                CreateCustomer(Constants.Application.FCMemberActiveStatus),
                CreateCustomer(Constants.Application.FCMemberInactiveStatus),
                new FlyingClubMember());

            var error = Assert.Single(response.ErrorDetails);

            Assert.Equal("G022", error.Code);
            Assert.Equal("Head of the account is not active", error.Message);
        }

        private static CustomerDTO CreateCustomer(string status) => new()
        {
            Customer = new CustomerCustMS
            {
                Status = status
            }
        };
    }
}
