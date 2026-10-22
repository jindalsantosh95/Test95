using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

public sealed class HouseHoldAccountMapperTests
{
    [Fact]
    public void RetrieveHouseHoldAccountDetails_SetsYoungFlyerFlagOnlyForManagedChild()
    {
        var customers = new[]
        {
            Customer("leader-id", "leader-fc"),
            Customer("young-flyer-id", "young-flyer-fc"),
            Customer("adult-id", "adult-fc")
        };

        var response = HouseHoldAccountMapper.RetrieveHouseHoldAccountDetails(
            customers,
            "leader-fc",
            new RetrieveHHAccountRequest { MemberType = HouseHoldMemberType.Household },
            "My household",
            "leader-fc",
            new[]
            {
                new FcMemberDetails
                {
                    FlyingClubMemberDocuments = customers.Select(customer => new FlyingClubMember
                    {
                        MemberNumber = customer.Customer.LoyaltyNumber,
                        TierCode = "Red"
                    })
                }
            },
            new VRedResponse { Current = "0" },
            Array.Empty<CustomerDTO>(),
            Array.Empty<FcMemberDetails>(),
            new HashSet<string>(["YOUNG-FLYER-ID"], StringComparer.OrdinalIgnoreCase));

        Assert.True(response.LinkedMemberInfo.Single(member => member.FCNumber == "young-flyer-fc").IsYoungFlyer);
        Assert.False(response.LinkedMemberInfo.Single(member => member.FCNumber == "adult-fc").IsYoungFlyer);
    }

    private static CustomerDTO Customer(string customerId, string loyaltyNumber) =>
        new()
        {
            Customer = new CustomerCustMS
            {
                CustomerId = customerId,
                LoyaltyNumber = loyaltyNumber,
                FirstName = "Test",
                LastName = "Member"
            }
        };
}
