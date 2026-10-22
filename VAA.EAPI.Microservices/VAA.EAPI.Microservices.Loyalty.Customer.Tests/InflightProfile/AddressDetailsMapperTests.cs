using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.InflightProfile;
using Xunit;
using ScvAddress = VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile.Address;
using ScvCustomer = VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile.Customer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.InflightProfile;

public sealed class AddressDetailsMapperTests
{
    [Theory]
    [InlineData("Y")]
    [InlineData("N")]
    public void MapAddresses_PreservesQasVerification(string qasVerified)
    {
        var customer = new ScvCustomer
        {
            AddressDetails =
            [
                new ScvAddress
                {
                    Primary = "Y",
                    Country = "United Kingdom",
                    QasVerified = qasVerified
                }
            ]
        };

        var result = AddressDetailsMapper.MapAddresses(
            customer,
            [new CountryDetails { Name = "GB", Value = "United Kingdom" }]);

        var address = Assert.Single(result);
        Assert.Equal(qasVerified, address.AddressVerified);
    }

    [Fact]
    public void MapAddresses_ReturnsNull_WhenNoPrimaryAddressExists()
    {
        var result = AddressDetailsMapper.MapAddresses(
            new ScvCustomer { AddressDetails = [new ScvAddress { Primary = "N" }] },
            []);

        Assert.Null(result);
    }
}
