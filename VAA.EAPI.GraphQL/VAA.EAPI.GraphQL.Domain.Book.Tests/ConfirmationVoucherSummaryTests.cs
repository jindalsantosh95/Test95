using System.Reflection;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Order;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests;

public sealed class ConfirmationVoucherSummaryTests
{
    [Fact]
    public void BasketSummary_ExposesVoucherRedemptionState()
    {
        var voucherDetails = new VoucherRedemptionDetails { RedemptionOptions = [] };
        var selection = new VoucherRedemptionSelection
        {
            SelectedVoucherIds = ["voucher-1"],
            SelectedOptionId = 1
        };

        var summary = new BasketSummary
        {
            HasSufficientPoints = true,
            VoucherPoints = "50000",
            PointsToPay = "25000",
            VoucherApplied = true,
            VoucherDetails = voucherDetails,
            VoucherRedemptionSelection = selection
        };

        Assert.True(summary.HasSufficientPoints);
        Assert.Equal("50000", summary.VoucherPoints);
        Assert.Equal("25000", summary.PointsToPay);
        Assert.True(summary.VoucherApplied);
        Assert.Same(voucherDetails, summary.VoucherDetails);
        Assert.Same(selection, summary.VoucherRedemptionSelection);
    }

    [Fact]
    public void BookBasket_HoldsVoucherStateAtTheRootLevel()
    {
        var basket = new Basket
        {
            HasSufficientPoints = true,
            VoucherPoints = "50000",
            PointsToPay = "25000",
            VoucherApplied = true,
            VoucherDetails = new VoucherRedemptionDetails { RedemptionOptions = [] },
            VoucherRedemptionSelection = new VoucherRedemptionSelection
            {
                SelectedVoucherIds = ["voucher-1"],
                SelectedOptionId = 1
            }
        };

        Assert.True(basket.HasSufficientPoints);
        Assert.Equal("50000", basket.VoucherPoints);
        Assert.Equal("25000", basket.PointsToPay);
        Assert.True(basket.VoucherApplied);
        Assert.NotNull(basket.VoucherDetails);
        Assert.Equal(["voucher-1"], basket.VoucherRedemptionSelection!.SelectedVoucherIds);
    }

    [Fact]
    public void AwardPayment_UsesVoucherPointsOnlyForAnExplicitValidatedSelection()
    {
        ConfigReader.Set(VAA.EAPI.GraphQL.Domain.Book.Constants.Application.VoucherRedemptionFeatureFlag, "true");
        var offer = new VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer.OfferResponse
        {
            Offers =
            [
                new VAA.EAPI.GraphQL.Domain.Book.Models.Offer.OfferDetails
                {
                    SalePrice =
                    [
                        new VAA.EAPI.GraphQL.Domain.Book.Models.Common.Price
                        {
                            PriceComponentType = VAA.EAPI.GraphQL.Domain.Book.Models.Common.Price.PriceComponentTypes.PointsEnum,
                            PointsAmount = new VAA.EAPI.GraphQL.Domain.Book.Models.Common.PointsAmount { PointsCount = 100000 }
                        }
                    ]
                }
            ]
        };
        var basket = new Basket
        {
            VoucherApplied = true,
            PointsToPay = "50000",
            VoucherDetails = new VoucherRedemptionDetails
            {
                RedemptionOptions =
                [
                    new RedemptionOption
                    {
                        OptionId = 1,
                        PointsToPay = 50000,
                        AppliedVouchers = [new AppliedVoucher { VoucherId = "voucher-1", PointsUsed = 50000 }]
                    }
                ]
            }
        };

        var getAwardPoints = typeof(CreateOrderMapper).GetMethod("GetAwardPointsToPay", BindingFlags.NonPublic | BindingFlags.Static)!;
        Assert.Equal("100000", getAwardPoints.Invoke(null, [offer, basket]));

        basket.VoucherRedemptionSelection = new VoucherRedemptionSelection
        {
            SelectedOptionId = 1,
            SelectedVoucherIds = ["voucher-1"]
        };
        Assert.Equal("50000", getAwardPoints.Invoke(null, [offer, basket]));
    }

    [Fact]
    public void GetConfirmationSummary_WhenVoucherRedemptionWasApplied_MapsVoucherDetailsToTheOrderResponse()
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
            VoucherApplied = true,
            HasSufficientPoints = true,
            VoucherPoints = "50000",
            PointsToPay = "50000",
            VoucherDetails = voucherDetails,
            VoucherRedemptionSelection = selection,
            FlightSummary = "{}"
        };

        var summary = CreateOrderMapper.GetConfirmationSummary(
            new BasketSummary(),
            new OrderResponseMS { OrderItems = [] },
            basket);

        Assert.True(summary.VoucherApplied);
        Assert.True(summary.HasSufficientPoints);
        Assert.Equal("50000", summary.VoucherAmount);
        Assert.Equal("50000", summary.VoucherPoints);
        Assert.Equal("50000", summary.PointsToPay);
        Assert.Same(voucherDetails, summary.VoucherDetails);
        Assert.Same(selection, summary.VoucherRedemptionSelection);
        var appliedVoucher = Assert.Single(summary.VoucherDetails.RedemptionOptions!.Single().AppliedVouchers!);
        Assert.Equal("TEST-VOUCHER-001", appliedVoucher.VoucherId);
        Assert.Equal(50000L, appliedVoucher.PointsUsed);
    }
}
