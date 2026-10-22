using VAA.EAPI.GraphQL.Domain.Search.Mapper;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using Basket = VAA.EAPI.GraphQL.Domain.Search.Models.Basket.Basket;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests;

public sealed class BasketRequestMapperTests
{
    [Fact]
    public void UpdateFromSummary_MapsVoucherStateAndPointsToTheBasket()
    {
        var selection = new VoucherRedemptionSelection
        {
            SelectedVoucherIds = ["voucher-1"],
            SelectedOptionId = 3
        };
        var summary = new Summary
        {
            HasSufficientPoints = true,
            VoucherApplied = true,
            VoucherRedemptionSelection = selection,
            Price = new Price { VoucherPoints = "40000", PointsToPay = "60000" }
        };

        var basket = BasketRequestMapper.UpdateFromSummary(
            new Basket(),
            new EarnPoints(),
            null!,
            [],
            summary,
            "summary-link");

        Assert.True(basket.VoucherApplied);
        Assert.True(basket.HasSufficientPoints);
        Assert.Equal("40000", basket.VoucherPoints);
        Assert.Equal("60000", basket.PointsToPay);
        Assert.Same(selection, basket.VoucherRedemptionSelection);
    }
}
