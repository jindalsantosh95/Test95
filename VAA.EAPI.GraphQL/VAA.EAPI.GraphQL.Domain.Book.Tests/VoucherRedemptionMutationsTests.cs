using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary;
using VAA.EAPI.GraphQL.Domain.Book.Models.Insurance;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Seat;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty;
using VAA.EAPI.GraphQL.Domain.Book.Mutations;
using VAA.EAPI.GraphQL.Domain.Book.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BookConstants = VAA.EAPI.GraphQL.Domain.Book.Constants.Application;
using BookEnvironment = VAA.EAPI.GraphQL.Domain.Book.Constants.Environment;
using TestHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests;

public sealed class VoucherRedemptionMutationsTests
{
    [Fact]
    public async Task SetVoucherRedemptionSelection_WhenFeatureIsDisabled_ReturnsNotFoundWithoutRetrievingBasket()
    {
        ConfigReader.Set(BookConstants.VoucherRedemptionFeatureFlag, "false");
        var mutation = CreateMutation(out _, enableVoucherRedemption: false);

        var exception = await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.SetVoucherRedemptionSelection(new SetVoucherRedemptionSelectionRequest
        {
            BasketId = "basket-id",
            SelectedVoucherIds = ["voucher-1"]
        }));

        Assert.Equal("404", exception.Errors.Single().Code);
    }

    [Fact]
    public async Task SetVoucherRedemptionSelection_SelectingDeselectingAndReselecting_RecalculatesAndPrefersUpgradeOnlyForTie()
    {
        using var mockDirectory = new CurrentDirectoryScope(AppContext.BaseDirectory);
        var mutation = CreateMutation(out _);

        var twoVoucherSelection = await mutation.SetVoucherRedemptionSelection(new SetVoucherRedemptionSelectionRequest
        {
            BasketId = "basket-id",
            SelectedVoucherIds = ["voucher-1", "voucher-2"]
        });

        Assert.True(twoVoucherSelection.VoucherApplied);
        Assert.Equal(2, twoVoucherSelection.VoucherDetails!.BestOption!.OptionId);
        Assert.Equal("UPGRADE", twoVoucherSelection.VoucherDetails.BestOption.RedemptionType);
        Assert.Equal("50000", twoVoucherSelection.VoucherPoints);
        Assert.Equal("50000", twoVoucherSelection.PointsToPay);
        Assert.True(twoVoucherSelection.HasSufficientPoints);

        var noVoucherSelection = await mutation.SetVoucherRedemptionSelection(new SetVoucherRedemptionSelectionRequest
        {
            BasketId = "basket-id",
            SelectedVoucherIds = []
        });

        Assert.False(noVoucherSelection.VoucherApplied);
        Assert.Null(noVoucherSelection.VoucherDetails!.BestOption);
        Assert.Empty(noVoucherSelection.VoucherRedemptionSelection!.SelectedVoucherIds!);
        Assert.Equal("0", noVoucherSelection.VoucherPoints);
        Assert.Equal("100000", noVoucherSelection.PointsToPay);
        Assert.False(noVoucherSelection.HasSufficientPoints);

        var oneVoucherSelection = await mutation.SetVoucherRedemptionSelection(new SetVoucherRedemptionSelectionRequest
        {
            BasketId = "basket-id",
            SelectedVoucherIds = ["voucher-1"]
        });

        Assert.True(oneVoucherSelection.VoucherApplied);
        Assert.Equal(3, oneVoucherSelection.VoucherDetails!.BestOption!.OptionId);
        Assert.Equal("COMPANION", oneVoucherSelection.VoucherDetails.BestOption.RedemptionType);
        Assert.Equal("40000", oneVoucherSelection.VoucherPoints);
        Assert.Equal("60000", oneVoucherSelection.PointsToPay);
        Assert.False(oneVoucherSelection.HasSufficientPoints);

    }

    [Fact]
    public async Task SetVoucherRedemptionSelection_PersistsSelectionAndRecalculatedValuesForTheNextBasketRetrieval()
    {
        using var mockDirectory = new CurrentDirectoryScope(AppContext.BaseDirectory);
        var mutationBasket = CreateBasket();
        var persistedBasket = CreateBasket();
        var basketProvider = new PersistingBasketServiceProviderFake(mutationBasket, persistedBasket);
        var mutation = CreateMutation(out _, basketProvider: basketProvider);

        await mutation.SetVoucherRedemptionSelection(new SetVoucherRedemptionSelectionRequest
        {
            BasketId = "basket-id",
            SelectedVoucherIds = ["voucher-1"]
        });

        var retrievedBasket = await basketProvider.RetrieveBasketAsync("basket-id");

        Assert.True(retrievedBasket.VoucherApplied);
        Assert.False(retrievedBasket.HasSufficientPoints);
        Assert.Equal("40000", retrievedBasket.VoucherPoints);
        Assert.Equal("60000", retrievedBasket.PointsToPay);
        Assert.Equal(3, retrievedBasket.VoucherRedemptionSelection!.SelectedOptionId);
        Assert.Equal(1, retrievedBasket.VoucherRedemptionSelection.MaxVouchersToUse);
        Assert.Equal(["voucher-1"], retrievedBasket.VoucherRedemptionSelection.SelectedVoucherIds);
        Assert.Equal(3, retrievedBasket.VoucherDetails!.BestOption!.OptionId);
    }

    [Fact]
    public async Task SetVoucherRedemptionSelection_SelectingMoreThanTwoVouchers_ReturnsBadRequest()
    {
        var mutation = CreateMutation(out _);

        var exception = await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => mutation.SetVoucherRedemptionSelection(new SetVoucherRedemptionSelectionRequest
        {
            BasketId = "basket-id",
            SelectedVoucherIds = ["voucher-1", "voucher-2", "voucher-3"]
        }));

        Assert.Equal("400", exception.Errors.Single().Code);
    }

    [Fact]
    public async Task SetVoucherRedemptionSelection_UsesTheSelectedOptionPriceInsteadOfStaleBasketTotals()
    {
        using var mockDirectory = new CurrentDirectoryScope(AppContext.BaseDirectory);
        var basket = CreateBasket();
        basket.VoucherPoints = "0";
        basket.PointsToPay = "125000";
        var basketProvider = new Mock<IBasketServiceProvider>();
        basketProvider.Setup(provider => provider.RetrieveBasketAsync("basket-id")).ReturnsAsync(basket);
        var mutation = CreateMutation(out _, basketProvider: basketProvider.Object);

        var result = await mutation.SetVoucherRedemptionSelection(new SetVoucherRedemptionSelectionRequest
        {
            BasketId = "basket-id",
            SelectedVoucherIds = ["voucher-1"]
        });

        Assert.Equal("60000", result.PointsToPay);
    }

    [Fact]
    public async Task UpdateVoucherRedemptionAsync_PostsTheVoucherSelectionToTheBasketEndpoint()
    {
        using var mockDirectory = new CurrentDirectoryScope(AppContext.BaseDirectory);
        ConfigReader.Set(BookEnvironment.BasketMSUrl, "https://testresources.virginatlantic.com/sit/basketmicroservice/basket");
        ConfigReader.Set(BookEnvironment.BasketMSOcpKey, "test-key");
        var httpClientFactory = new TestHttpClientFactory();
        var provider = new BasketServiceProvider(new HttpConnector(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance,
            httpClientFactory));

        await provider.UpdateVoucherRedemptionAsync("basket-id", new VoucherRedemptionUpdateRequest
        {
            HasSufficientPoints = false,
            VoucherPoints = "40000",
            PointsToPay = "60000",
            VoucherApplied = true,
            VoucherRedemptionSelection = new VoucherRedemptionSelection
            {
                SelectedOptionId = 3,
                MaxVouchersToUse = 1,
                SelectedVoucherIds = ["voucher-1"]
            }
        });

        var response = Assert.Single(httpClientFactory.GetResponses("https://testresources.virginatlantic.com"));
        var request = Assert.IsType<HttpRequestMessage>(response.RequestMessage);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/sit/basketmicroservice/basket/basket-id/voucherredemption", request.RequestUri!.AbsolutePath);

        var content = await request.Content!.ReadAsStringAsync();
        Assert.Contains("\"voucherApplied\":true", content);
        Assert.Contains("\"selectedOptionId\":3", content);
        Assert.Contains("\"maxVouchersToUse\":1", content);
        Assert.Contains("\"selectedVoucherIds\":[\"voucher-1\"]", content);
    }

    private const string BasketUrl = "https://testresources.virginatlantic.com/sit/basketmicroservice/basket/basket-id";

    private static VoucherRedemptionMutations CreateMutation(out TestHttpClientFactory httpClientFactory, bool enableVoucherRedemption = true, IBasketServiceProvider? basketProvider = null)
    {
        ConfigReader.Set(BookConstants.VoucherRedemptionFeatureFlag, enableVoucherRedemption.ToString());
        ConfigReader.Set(BookEnvironment.BasketMSRetrieveUrl, "https://testresources.virginatlantic.com/sit/basketmicroservice/basket/{basket}");
        ConfigReader.Set(BookEnvironment.BasketMSUrl, "https://testresources.virginatlantic.com/sit/basketmicroservice/basket");
        ConfigReader.Set(BookEnvironment.BasketMSOcpKey, "test-key");

        httpClientFactory = new TestHttpClientFactory();
        var tokenB2CProvider = new Mock<ITokenB2CProvider<string>>();
        tokenB2CProvider
            .Setup(provider => provider.ValidateTokenAsync("test-token"))
            .ReturnsAsync("{\"fcNumber\":\"1234567890\"}");

        AsyncContext.Current = new AsyncContext
        {
            Context = new TestFunctionContext(new Dictionary<string, object?>
            {
                ["Headers"] = "{\"Authorization\":\"Bearer test-token\"}"
            })
        };

        return new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory>(httpClientFactory)
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IBasketServiceProvider>(basketProvider ?? CreateBasketProvider())
            .AddSingleton<IVRedServiceProvider>(new VRedServiceProviderStub("50000"))
            .AddSingleton<ITokenB2CProvider<string>>(tokenB2CProvider.Object)
            .AddSingleton<VoucherRedemptionMutations>()
            .BuildServiceProvider()
            .GetRequiredService<VoucherRedemptionMutations>();
    }

    private static IBasketServiceProvider CreateBasketProvider()
    {
        var basketProvider = new Mock<IBasketServiceProvider>();
        basketProvider.Setup(provider => provider.RetrieveBasketAsync("basket-id")).ReturnsAsync(CreateBasket());
        return basketProvider.Object;
    }

    private sealed class PersistingBasketServiceProviderFake(Basket mutationBasket, Basket persistedBasket) : IBasketServiceProvider
    {
        private int _retrieveCount;

        public Task<Basket> RetrieveBasketAsync(string basketId) =>
            Task.FromResult(Interlocked.Increment(ref _retrieveCount) == 1 ? mutationBasket : persistedBasket);
        public Task<Basket> CreateBasketAsync(CreateBasketRequest createBasketRequest) => Task.FromResult(mutationBasket);
        public Task<Basket> UpdateContactAsync(ContactUpdateRequest contactDetails) => Task.FromResult(mutationBasket);
        public Task<Basket> UpdatePeopleAsync(PassengerUpdateRequest people) => Task.FromResult(mutationBasket);
        public Task<Basket> UpdateTnCAsync(TnCUpdateRequest termsAndConditions) => Task.FromResult(mutationBasket);
        public Task<Basket> UpdatePaymentDetailsAsync(BasketPaymentRequest basketPaymentRequest) => Task.FromResult(mutationBasket);
        public Task<bool> DeleteBasketAsync(string basketId) => Task.FromResult(false);
        public Task<Basket> UpdateBasketOrderStatusAsync(string basketId, string status) => Task.FromResult(mutationBasket);
        public Task<Basket> UpdateSeatDetailsAsync(SeatUpdateRequest seatUpdateRequest) => Task.FromResult(mutationBasket);
        public Task<Basket> UpdateCustomerProfileAsync(string basketId, BasketLoyaltyMemberDetails loyaltyMemberDetails) => Task.FromResult(mutationBasket);
        public Task<Basket> UpdateFCEarnPointsAsync(string basketId, FCEarnPoints fCEarnPoints) => Task.FromResult(mutationBasket);
        public Task UpdateVoucherRedemptionAsync(string basketId, VoucherRedemptionUpdateRequest request)
        {
            persistedBasket.HasSufficientPoints = request.HasSufficientPoints;
            persistedBasket.VoucherPoints = request.VoucherPoints;
            persistedBasket.PointsToPay = request.PointsToPay;
            persistedBasket.VoucherApplied = request.VoucherApplied;
            persistedBasket.VoucherDetails = request.VoucherDetails;
            persistedBasket.VoucherRedemptionSelection = request.VoucherRedemptionSelection;
            return Task.CompletedTask;
        }

        public Task<Basket> UpdatePointsCurrencyDetailsAsync(string basketId, IEnumerable<PtcWisePointsEquivalentCurrencyDetails> pointsCurrencyDetails) => Task.FromResult(mutationBasket);
        public Task<Basket> SaveInsuranceQuoteAsync(string basketId, IEnumerable<QuoteResponse> quoteResponse) => Task.FromResult(mutationBasket);
        public Task<Basket> UpdateInsuranceDecisionAsync(string basketId, bool insuranceSelected) => Task.FromResult(mutationBasket);
        public Task<Basket> SetCobrandProgress(string basketId, string cobrandProgress) => Task.FromResult(mutationBasket);
        public Task<Basket> UpdateFlightSummaryAsync(string basketId, string flightSummary, string summaryLink) => Task.FromResult(mutationBasket);
    }

    private static VAA.EAPI.GraphQL.Domain.Book.Models.Basket.Basket CreateBasket() => new()
    {
        CustomerProfile = new VAA.EAPI.GraphQL.Domain.Book.Models.Basket.BasketLoyaltyMemberDetails
        {
            MembershipDetails = new VAA.EAPI.GraphQL.Domain.Book.Models.Basket.FQTMembership { FQTNumber = "1234567890" }
        },
        VoucherPoints = "0",
        PointsToPay = "100000",
        VoucherDetails = new VoucherRedemptionDetails
        {
            RedemptionOptions =
            [
                new RedemptionOption { OptionId = 1, RedemptionType = "COMPANION", PointsToPay = 60000, TotalPointsUsed = 40000, AppliedVouchers = [new AppliedVoucher { VoucherId = "voucher-2", PointsUsed = 40000 }] },
                new RedemptionOption { OptionId = 2, RedemptionType = "UPGRADE", PointsToPay = 50000, TotalPointsUsed = 50000, AppliedVouchers = [new AppliedVoucher { VoucherId = "voucher-1", PointsUsed = 25000 }, new AppliedVoucher { VoucherId = "voucher-2", PointsUsed = 25000 }] },
                new RedemptionOption { OptionId = 3, RedemptionType = "COMPANION", PointsToPay = 60000, TotalPointsUsed = 40000, AppliedVouchers = [new AppliedVoucher { VoucherId = "voucher-1", PointsUsed = 40000 }] }
            ]
        }
    };

    private sealed class VRedServiceProviderStub(string currentPoints) : IVRedServiceProvider
    {
        public Task<VRedResponse> GetVirginPoints(string fqtNumber) =>
            Task.FromResult(new VRedResponse { Current = currentPoints });
    }

    private sealed class CurrentDirectoryScope : IDisposable
    {
        private readonly string _previousDirectory = Directory.GetCurrentDirectory();

        public CurrentDirectoryScope(string directory)
        {
            Directory.SetCurrentDirectory(directory);
        }

        public void Dispose() => Directory.SetCurrentDirectory(_previousDirectory);
    }
}
