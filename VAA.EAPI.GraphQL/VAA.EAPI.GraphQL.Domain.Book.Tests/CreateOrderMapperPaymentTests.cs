using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Payment;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests;

public sealed class CreateOrderMapperPaymentTests
{
    [Fact]
    public void GetPaymentDetails_WhenCustomerProfileNamesAreMissing_UsesPassengerMatchingLoyaltyAccount()
    {
        var basket = CreateAwardBasket(customerProfileFirstName: null, customerProfileLastName: string.Empty);

        var paymentDetail = Assert.Single(CreateOrderMapper.GetPaymentDetails(CreateAwardOffer(), basket, new PaymentResponse { PaymentDetails = [] }, "payment-reference"));

        Assert.Equal("Profile", paymentDetail.Payer.FirstName);
        Assert.Equal("Passenger", paymentDetail.Payer.LastName);
    }

    [Fact]
    public void GetPaymentDetails_WhenCustomerProfileNamesArePresent_PrefersCustomerProfile()
    {
        var basket = CreateAwardBasket(customerProfileFirstName: "Account", customerProfileLastName: "Holder");

        var paymentDetail = Assert.Single(CreateOrderMapper.GetPaymentDetails(CreateAwardOffer(), basket, new PaymentResponse { PaymentDetails = [] }, "payment-reference"));

        Assert.Equal("Account", paymentDetail.Payer.FirstName);
        Assert.Equal("Holder", paymentDetail.Payer.LastName);
    }

    [Fact]
    public void GetPaymentDetails_WhenNoLoyaltyPayerNameIsAvailable_ThrowsBeforeVoucherRedemption()
    {
        var basket = CreateAwardBasket(customerProfileFirstName: null, customerProfileLastName: null);
        basket.People = [];

        var exception = Assert.Throws<ArgumentException>(() =>
            CreateOrderMapper.GetPaymentDetails(CreateAwardOffer(), basket, new PaymentResponse { PaymentDetails = [] }, "payment-reference").ToList());

        Assert.Equal("Loyalty member first name and last name are required for an award booking.", exception.Message);
    }

    private static Basket CreateAwardBasket(string customerProfileFirstName, string customerProfileLastName) => new()
    {
        FlightsearchRequest = new FlightsearchRequest { isAwardSearch = true },
        CustomerProfile = new BasketLoyaltyMemberDetails
        {
            FirstName = customerProfileFirstName,
            LastName = customerProfileLastName,
            MembershipDetails = new FQTMembership { FQTNumber = "1234567890" }
        },
        People =
        [
            new BasketPerson
            {
                Id = "0",
                FirstName = "Profile",
                LastName = "Passenger",
                ProfilePassenger = true,
                MembershipDetails = new FQTMembership { FQTNumber = "1234567890" }
            }
        ]
    };

    private static OfferResponse CreateAwardOffer() => new()
    {
        Offers =
        [
            new OfferDetails
            {
                SalePrice =
                [
                    new Price
                    {
                        PriceComponentType = Price.PriceComponentTypes.PointsEnum,
                        PointsAmount = new PointsAmount { PointsCount = 50000 }
                    }
                ]
            }
        ]
    };
}
