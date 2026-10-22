using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary;
using VAA.EAPI.GraphQL.Domain.Book.Models.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using Xunit;
using OfferPrice = VAA.EAPI.GraphQL.Domain.Book.Models.Common.Price;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests;

public sealed class CreateOrderMapperVoucherTests
{
    [Fact]
    public void GetVoucherDetails_WhenVoucherOptionIsSelected_MapsSelectedVouchers()
    {
        var basket = new Basket
        {
            VoucherApplied = true,
            CustomerProfile = new BasketLoyaltyMemberDetails
            {
                MembershipDetails = new FQTMembership { FQTNumber = "1234567890" }
            },
            VoucherRedemptionSelection = new VoucherRedemptionSelection { SelectedOptionId = 2 },
            VoucherDetails = new VoucherRedemptionDetails
            {
                BestOption = new BestRedemptionOption { OptionId = 2 },
                RedemptionOptions =
                [
                    new RedemptionOption
                    {
                        OptionId = 2,
                        TotalPointsUsed = 55000,
                        AppliedVouchers = [new AppliedVoucher { VoucherId = "TEST-VOUCHER-001", PointsUsed = 55000 }]
                    }
                ]
            }
        };

        var voucherDetails = CreateOrderMapper.GetVoucherDetails(basket);

        Assert.NotNull(voucherDetails);
        Assert.Equal("1234567890", voucherDetails.VoucherOwnerFcAccount);
        Assert.Equal(55000L, voucherDetails.TotalPointsUsed);
        var voucher = Assert.Single(voucherDetails.AppliedVouchers);
        Assert.Equal("TEST-VOUCHER-001", voucher.VoucherId);
        Assert.Equal(55000L, voucher.PointsUsed);
    }

    [Fact]
    public void GetVoucherDetails_WhenVoucherIsPartiallyApplied_UsesOriginalOfferPointsForTotal()
    {
        var basket = new Basket
        {
            VoucherApplied = true,
            VoucherRedemptionSelection = new VoucherRedemptionSelection { SelectedOptionId = 1 },
            VoucherDetails = new VoucherRedemptionDetails
            {
                RedemptionOptions =
                [
                    new RedemptionOption
                    {
                        OptionId = 1,
                        TotalPointsUsed = 90000,
                        PointsToPay = 30000,
                        AppliedVouchers = [new AppliedVoucher { VoucherId = "TEST-VOUCHER-001", PointsUsed = 90000 }]
                    }
                ]
            }
        };
        var offerResponse = new OfferResponse
        {
            Offers =
            [
                new OfferDetails
                {
                    SalePrice =
                    [
                        new OfferPrice
                        {
                            PriceComponentType = OfferPrice.PriceComponentTypes.PointsEnum,
                            PointsAmount = new PointsAmount { PointsCount = 120000 }
                        }
                    ]
                }
            ]
        };

        var voucherDetails = CreateOrderMapper.GetVoucherDetails(basket, offerResponse);

        Assert.NotNull(voucherDetails);
        Assert.Equal(120000L, voucherDetails.TotalPointsUsed);
    }

    [Fact]
    public void GetVoucherDetails_WhenNoVoucherOptionIsSelected_ReturnsNull()
    {
        var basket = new Basket
        {
            VoucherApplied = true,
            VoucherDetails = new VoucherRedemptionDetails
            {
                BestOption = new BestRedemptionOption { OptionId = 2 },
                RedemptionOptions =
                [
                    new RedemptionOption
                    {
                        OptionId = 2,
                        AppliedVouchers = [new AppliedVoucher { VoucherId = "BEST-OPTION", PointsUsed = 55000 }]
                    }
                ]
            }
        };

        var voucherDetails = CreateOrderMapper.GetVoucherDetails(basket);

        Assert.Null(voucherDetails);
    }

    [Fact]
    public void GetVoucherDetails_WhenSelectedOptionDiffersFromBestOption_MapsSelectedVoucher()
    {
        var basket = new Basket
        {
            VoucherApplied = true,
            VoucherRedemptionSelection = new VoucherRedemptionSelection { SelectedOptionId = 2 },
            VoucherDetails = new VoucherRedemptionDetails
            {
                BestOption = new BestRedemptionOption { OptionId = 1 },
                RedemptionOptions =
                [
                    new RedemptionOption { OptionId = 1, AppliedVouchers = [new AppliedVoucher { VoucherId = "BEST", PointsUsed = 1000 }] },
                    new RedemptionOption { OptionId = 2, AppliedVouchers = [new AppliedVoucher { VoucherId = "SELECTED", PointsUsed = 2000 }] }
                ]
            }
        };

        var voucherDetails = CreateOrderMapper.GetVoucherDetails(basket);

        Assert.Equal("SELECTED", Assert.Single(voucherDetails.AppliedVouchers).VoucherId);
    }

    [Fact]
    public void GetVoucherDetails_WhenSelectionExistsOnlyInFlightSummary_MapsSelectedVoucher()
    {
        var basket = new Basket
        {
            VoucherApplied = true,
            FlightSummary = "{\"voucherRedemptionSelection\":{\"selectedOptionId\":2,\"selectedVoucherIds\":[\"SUMMARY-SELECTED\"]}}",
            VoucherDetails = new VoucherRedemptionDetails
            {
                RedemptionOptions =
                [
                    new RedemptionOption { OptionId = 1, AppliedVouchers = [new AppliedVoucher { VoucherId = "OTHER", PointsUsed = 1000 }] },
                    new RedemptionOption { OptionId = 2, AppliedVouchers = [new AppliedVoucher { VoucherId = "SUMMARY-SELECTED", PointsUsed = 2000 }] }
                ]
            }
        };

        var voucherDetails = CreateOrderMapper.GetVoucherDetails(basket);

        Assert.Equal("SUMMARY-SELECTED", Assert.Single(voucherDetails.AppliedVouchers).VoucherId);
    }
}
