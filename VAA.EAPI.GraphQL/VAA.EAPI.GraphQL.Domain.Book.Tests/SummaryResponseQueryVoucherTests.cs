using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary;
using VAA.EAPI.GraphQL.Domain.Book.Models.Insurance;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty;
using VAA.EAPI.GraphQL.Domain.Book.Queries;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests;

public sealed class SummaryResponseQueryVoucherTests
{
    [Fact]
    public async Task GetFlightSummaryResponseAsync_MapsPersistedVoucherRedemptionFromTheBasket()
    {
        var voucherDetails = new VoucherRedemptionDetails
        {
            RedemptionOptions =
            [
                new RedemptionOption
                {
                    OptionId = 7,
                    AppliedVouchers = [new AppliedVoucher { VoucherId = "TEST-VOUCHER-001", PointsUsed = 50000 }]
                }
            ]
        };
        var selection = new VoucherRedemptionSelection
        {
            SelectedOptionId = 7,
            SelectedVoucherIds = ["TEST-VOUCHER-001"]
        };
        var basket = new Basket
        {
            HasSufficientPoints = true,
            VoucherApplied = true,
            VoucherPoints = "50000",
            PointsToPay = "50000",
            VoucherDetails = voucherDetails,
            VoucherRedemptionSelection = selection,
            FlightSummary = Json.Serialize(new Summary())
        };
        var query = new SummaryResponseQuery(NullLogger<SummaryResponseQuery>.Instance, new BasketServiceProviderStub(basket));

        var response = await query.GetFlightSummaryResponseAsync("basket-id");

        Assert.True(response.VoucherApplied);
        Assert.True(response.HasSufficientPoints);
        Assert.Equal("50000", response.VoucherPoints);
        Assert.Equal("50000", response.PointsToPay);
        Assert.Same(voucherDetails, response.VoucherDetails);
        Assert.Same(selection, response.VoucherRedemptionSelection);
        var appliedVoucher = Assert.Single(response.VoucherDetails!.RedemptionOptions!.Single().AppliedVouchers!);
        Assert.Equal("TEST-VOUCHER-001", appliedVoucher.VoucherId);
        Assert.Equal(50000L, appliedVoucher.PointsUsed);
    }

    private sealed class BasketServiceProviderStub(Basket basket) : IBasketServiceProvider
    {
        public Task<Basket> RetrieveBasketAsync(string offerId) => Task.FromResult(basket);
        public Task<Basket> CreateBasketAsync(CreateBasketRequest createBasketRQ) => Task.FromResult(basket);
        public Task<Basket> UpdateContactAsync(ContactUpdateRequest contactDetails) => Task.FromResult(basket);
        public Task<Basket> UpdatePeopleAsync(PassengerUpdateRequest people) => Task.FromResult(basket);
        public Task<Basket> UpdateTnCAsync(TnCUpdateRequest termsAndConditions) => Task.FromResult(basket);
        public Task<Basket> UpdatePaymentDetailsAsync(BasketPaymentRequest basketPaymentRequest) => Task.FromResult(basket);
        public Task<bool> DeleteBasketAsync(string basketId) => Task.FromResult(false);
        public Task<Basket> UpdateBasketOrderStatusAsync(string basketid, string status) => Task.FromResult(basket);
        public Task<Basket> UpdateSeatDetailsAsync(SeatUpdateRequest seatUpdateRequest) => Task.FromResult(basket);
        public Task<Basket> UpdateCustomerProfileAsync(string basketId, BasketLoyaltyMemberDetails loyaltyMemberDetails) => Task.FromResult(basket);
        public Task<Basket> UpdateFCEarnPointsAsync(string basketId, FCEarnPoints fCEarnPoints) => Task.FromResult(basket);
        public Task UpdateVoucherRedemptionAsync(string basketId, VoucherRedemptionUpdateRequest request) => Task.CompletedTask;
        public Task<Basket> UpdatePointsCurrencyDetailsAsync(string basketId, IEnumerable<PtcWisePointsEquivalentCurrencyDetails> pointsCurrencyDetails) => Task.FromResult(basket);
        public Task<Basket> SaveInsuranceQuoteAsync(string basketId, IEnumerable<QuoteResponse> quoteResponse) => Task.FromResult(basket);
        public Task<Basket> UpdateInsuranceDecisionAsync(string basketId, bool insuranceAdded) => Task.FromResult(basket);
        public Task<Basket> SetCobrandProgress(string basketId, string cobrandProgress) => Task.FromResult(basket);
        public Task<Basket> UpdateFlightSummaryAsync(string basketId, string flightSummary, string summaryLink) => Task.FromResult(basket);
    }
}
