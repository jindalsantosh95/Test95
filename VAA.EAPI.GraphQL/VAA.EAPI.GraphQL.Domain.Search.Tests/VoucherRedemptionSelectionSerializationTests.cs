using System.Linq;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{
    public sealed class VoucherRedemptionSelectionSerializationTests
    {
        [Fact]
        public void Summary_RecalculatesSufficientPointsAfterApplyingBestVoucherOption()
        {
            var summary = new Summary
            {
                Price = new Price { AwardPoints = "120000" }
            };

            summary.ApplyVoucherDetails(new VoucherRedemptionDetails
            {
                RedemptionOptions =
                [
                    new RedemptionOption
                    {
                        OptionId = 1,
                        TotalPointsUsed = 60000,
                        PointsToPay = 60000,
                        AppliedVouchers = [new AppliedVoucher { VoucherId = "V1", PointsUsed = 60000 }]
                    }
                ],
                BestOption = new BestRedemptionOption { OptionId = 1 }
            }, "80000");

            Assert.True(summary.VoucherApplied);
            Assert.NotNull(summary.VoucherRedemptionSelection);
            Assert.Equal(1, summary.VoucherRedemptionSelection.SelectedOptionId);
            Assert.Equal("V1", summary.VoucherRedemptionSelection.SelectedVoucherIds!.Single());
            Assert.Equal("60000", summary.Price.PointsToPay);
            Assert.True(summary.HasSufficientPoints);
        }

        [Fact]
        public void Summary_SetsHasSufficientPointsFalse_WhenSelectedVoucherOptionLeavesAPointsShortfall()
        {
            var summary = new Summary
            {
                Price = new Price { AwardPoints = "100000" },
                VoucherRedemptionSelection = new VoucherRedemptionSelection
                {
                    SelectedVoucherIds = ["V2"],
                    SelectedOptionId = 2,
                }
            };

            summary.ApplyVoucherDetails(new VoucherRedemptionDetails
            {
                RedemptionOptions =
                [
                    new RedemptionOption { OptionId = 1, TotalPointsUsed = 40000, PointsToPay = 60000 },
                    new RedemptionOption { OptionId = 2, TotalPointsUsed = 60000, PointsToPay = 40000 },
                ],
                BestOption = new BestRedemptionOption { OptionId = 1 }
            }, "30000");

            Assert.True(summary.VoucherApplied);
            Assert.Equal("60000", summary.Price.VoucherPoints);
            Assert.Equal("40000", summary.Price.PointsToPay);
            Assert.False(summary.HasSufficientPoints);
        }

        [Fact]
        public void Summary_PreservesInfantAwardPoints_WhenApplyingVoucherDetails()
        {
            var summary = new Summary
            {
                Price = new Price { AwardPoints = "110000" },
                FareCalculationSummary = new FareCalculationSummary { TotalAwardPoints = "105000" }
            };

            summary.ApplyVoucherDetails(new VoucherRedemptionDetails
            {
                RedemptionOptions = [new RedemptionOption { OptionId = 1, TotalPointsUsed = 44000, PointsToPay = 66000 }],
                BestOption = new BestRedemptionOption { OptionId = 1 }
            });

            Assert.Equal("110000", summary.Price.AwardPoints);
            Assert.Equal("44000", summary.Price.VoucherPoints);
            Assert.Equal("66000", summary.Price.PointsToPay);
        }

        [Fact]
        public void Summary_UsesFareCalculationAwardPoints_WhenPriceAwardPointsAreUnavailable()
        {
            var summary = new Summary
            {
                Price = new Price(),
                FareCalculationSummary = new FareCalculationSummary { TotalAwardPoints = "105000" }
            };

            summary.ApplyVoucherDetails(new VoucherRedemptionDetails
            {
                RedemptionOptions = [new RedemptionOption { OptionId = 1, TotalPointsUsed = 44000, PointsToPay = 61000 }],
                BestOption = new BestRedemptionOption { OptionId = 1 }
            });

            Assert.Equal("105000", summary.Price.AwardPoints);
            Assert.Equal("44000", summary.Price.VoucherPoints);
            Assert.Equal("61000", summary.Price.PointsToPay);
        }

        [Fact]
        public void RedemptionOption_SerializesBaseLinePointsForUpgradePresentation()
        {
            var json = Json.Serialize(new RedemptionOption
            {
                RedemptionType = "UPGRADE",
                BaselinePoints = 40000
            });

            Assert.Contains("\"baseLinePoints\":40000", json);
        }

        [Fact]
        public void Summary_SerializesAndDeserializes_VoucherSelection()
        {
            var summary = new Summary
            {
                PassengerCounts = "a2t0c0i0",
                VoucherDetails = new VoucherRedemptionDetails
                {
                    AvailableVouchers =
                    [
                        new AvailableVoucher { VoucherId = "V1", Type = "REWARD", BalancePoints = 1000, ExpiryDate = new System.DateTime(2027, 1, 1) },
                    ],
                    BestOption = new BestRedemptionOption { OptionId = 1 },
                },
                VoucherRedemptionSelection = new VoucherRedemptionSelection
                {
                    MaxVouchersToUse = 1,
                    SelectedVoucherIds = new[] { "V1" },
                    SelectedOptionId = 1,
                }
            };

            var json = Json.Serialize(summary);
            var roundTrip = Json.Deserialize<Summary>(json);

            Assert.NotNull(roundTrip);
            Assert.NotNull(roundTrip.VoucherRedemptionSelection);
            Assert.Equal(1, roundTrip.VoucherRedemptionSelection.MaxVouchersToUse);
            Assert.Equal("V1", roundTrip.VoucherRedemptionSelection.SelectedVoucherIds.Single());
            Assert.Equal(1, roundTrip.VoucherRedemptionSelection.SelectedOptionId);
            Assert.NotNull(roundTrip.VoucherDetails);
            Assert.Equal("V1", roundTrip.VoucherDetails.AvailableVouchers.Single().VoucherId);
            Assert.Equal("REWARD", roundTrip.VoucherDetails.AvailableVouchers.Single().Type);
            Assert.Equal(1, roundTrip.VoucherDetails.BestOption.OptionId);
        }
    }
}

