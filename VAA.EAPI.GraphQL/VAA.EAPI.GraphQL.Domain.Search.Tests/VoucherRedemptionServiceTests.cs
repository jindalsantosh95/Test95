using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Search.Helpers;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using Xunit;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{
    public sealed class VoucherRedemptionServiceTests
    {
        [Fact]
        public async Task CoversTwoCompanionPassengers_WhenOneVoucherHasEnoughPointsForBoth()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                [
                    new VoucherDetail { VoucherNumber = "V-75000-A", MaxValue = "75000" },
                    new VoucherDetail { VoucherNumber = "V-75000-B", MaxValue = "75000" }
                ],
                eligibleCompanionPaxCount: 2,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 19001,
                totalAwardPointsNeededToPay: 38002);

            Assert.NotNull(result);
            var options = result.Value.RedemptionOptions.ToList();

            Assert.Equal(2, options.Count);
            Assert.All(options, option =>
            {
                Assert.Single(option.AppliedVouchers);
                Assert.Equal(19001, option.TotalPointsUsed);
                Assert.Equal(19001, option.PointsToPay);
                Assert.Equal(2, option.CoveredPassengers);
            });
        }

        [Fact]
        public async Task ReturnsTwoVoucherOption_WhenTwoCompanionSeatsAreEligible()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var vouchers = new List<VoucherDetail>
            {
                new() { VoucherNumber = "V-0495-A", MaxValue = "495" },
                new() { VoucherNumber = "V-0495-B", MaxValue = "495" },
            };

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                vouchers,
                eligibleCompanionPaxCount: 2,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 990,
                totalAwardPointsNeededToPay: 1980);

            Assert.NotNull(result);

            var options = result.Value.RedemptionOptions.ToList();
            Assert.Equal(3, options.Count);

            Assert.Equal(1485, options[0].PointsToPay);
            Assert.Equal(1485, options[1].PointsToPay);

            var bestOption = options.Single(o => o.OptionId == result.Value.BestOption.OptionId);
            Assert.Equal(2, bestOption.CoveredPassengers);
            Assert.Equal(990, bestOption.PointsToPay);
            Assert.Equal(2, bestOption.AppliedVouchers.Count());
            Assert.All(bestOption.AppliedVouchers, voucher => Assert.Equal(495, voucher.PointsUsed));
        }

        [Fact]
        public async Task ReturnsSingleRecommendedOption_WhenCustomerRestrictsToOneVoucher()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var vouchers = new List<VoucherDetail>
            {
                new() { VoucherNumber = "V-2000", MaxValue = "2000" },
                new() { VoucherNumber = "V-1000", MaxValue = "1000" },
            };

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                vouchers,
                eligibleCompanionPaxCount: 2,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 990,
                totalAwardPointsNeededToPay: 1980,
                maxVouchersToUse: 1);

            Assert.NotNull(result);

            var options = result.Value.RedemptionOptions.ToList();
            Assert.Single(options);
            Assert.Equal(2, options[0].CoveredPassengers);
            Assert.Equal("V-1000", options[0].AppliedVouchers.Single().VoucherId);
        }

        [Fact]
        public async Task UsesRequestedVoucher_WhenCustomerSelectsSpecificVoucher()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var vouchers = new List<VoucherDetail>
            {
                new() { VoucherNumber = "V-2000", MaxValue = "2000" },
                new() { VoucherNumber = "V-1000", MaxValue = "1000" },
            };

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                vouchers,
                eligibleCompanionPaxCount: 2,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 990,
                totalAwardPointsNeededToPay: 1980,
                maxVouchersToUse: 1,
                voucherIdToUse: "V-2000");

            Assert.NotNull(result);

            var option = result.Value.RedemptionOptions.Single();
            Assert.Equal("V-2000", option.AppliedVouchers.Single().VoucherId);
        }

        [Fact]
        public async Task ReturnsNull_WhenNoEligibleCompanions()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                availableVouchers: new List<VoucherDetail>
                {
                    new() { VoucherNumber = "V1", MaxValue = "1000" },
                },
                eligibleCompanionPaxCount: 0,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 1000,
                totalAwardPointsNeededToPay: 2000);

            Assert.Null(result);
        }

        [Fact]
        public async Task ReturnsNull_WhenNoVouchersAvailable()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                availableVouchers: new List<VoucherDetail>(),
                eligibleCompanionPaxCount: 1,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 1000,
                totalAwardPointsNeededToPay: 2000);

            Assert.Null(result);
        }

        [Fact]
        public async Task DoesNotOfferTwoCompanionVouchers_WhenOnlyOneCompanionSeatIsEligible()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");
            var vouchers = new List<VoucherDetail>
            {
                new() { VoucherNumber = "V-1000", MaxValue = "1000" },
                new() { VoucherNumber = "V-2000", MaxValue = "2000" }
            };

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                vouchers,
                eligibleCompanionPaxCount: 1,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 2000,
                totalAwardPointsNeededToPay: 6000);

            var option = Assert.Single(result!.Value.RedemptionOptions);
            Assert.Single(option.AppliedVouchers!);
            Assert.Equal(1, option.CoveredPassengers);
        }

        [Fact]
        public async Task CoversBothCompanionPassengers_WhenOneVoucherHasSufficientPoints()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                [new VoucherDetail { VoucherNumber = "V-75000", MaxValue = "75000" }],
                eligibleCompanionPaxCount: 2,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 50000,
                totalAwardPointsNeededToPay: 100000,
                maxVouchersToUse: 1);

            var option = Assert.Single(result!.Value.RedemptionOptions);
            Assert.Single(option.AppliedVouchers!);
            Assert.Equal(2, option.CoveredPassengers);
            Assert.Equal(50000, option.AppliedVouchers.Single().PointsUsed);
            Assert.Equal(50000, option.PointsToPay);
        }

        [Fact]
        public async Task SelectsSmallestVoucherThatCoversNeededPoints()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var vouchers = new List<VoucherDetail>
            {
                new() { VoucherNumber = "V-2000", MaxValue = "2000" },
                new() { VoucherNumber = "V-1000", MaxValue = "1000" },
            };

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                vouchers,
                eligibleCompanionPaxCount: 1,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 990,
                totalAwardPointsNeededToPay: 1980);

            Assert.NotNull(result);

            var options = result.Value.RedemptionOptions.ToList();

            var best = result.Value.BestOption;
            Assert.Equal("V-1000", options.First(o => o.OptionId == best.OptionId).AppliedVouchers.Single().VoucherId);
            Assert.Equal(990, options.First(o => o.OptionId == best.OptionId).AppliedVouchers.Single().PointsUsed);
        }

        [Fact]
        public async Task UsesHighestValueVoucher_WhenSingleVoucherCannotExactlyMatchNeededPoints()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var vouchers = new List<VoucherDetail>
            {
                new() { VoucherNumber = "V-1000", MaxValue = "1000" },
                new() { VoucherNumber = "V-2000", MaxValue = "2000" },
            };

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                vouchers,
                eligibleCompanionPaxCount: 1,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 1800,
                totalAwardPointsNeededToPay: 3600);

            Assert.NotNull(result);

            var best = result.Value.BestOption;
            var bestOption = result.Value.RedemptionOptions.First(o => o.OptionId == best.OptionId);
            Assert.Equal("V-2000", bestOption.AppliedVouchers.Single().VoucherId);
            Assert.Equal(1800, bestOption.AppliedVouchers.Single().PointsUsed);
        }

        [Fact]
        public async Task CapsCompanionCoverageAtTheCompanionFare_WhenVoucherValueIsHigher()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                [new VoucherDetail { VoucherNumber = "V-75000", MaxValue = "75000" }],
                eligibleCompanionPaxCount: 1,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 25000,
                totalAwardPointsNeededToPay: 50000);

            Assert.NotNull(result);
            var option = Assert.Single(result.Value.RedemptionOptions);

            Assert.Equal(25000, option.AppliedVouchers.Single().PointsUsed);
            Assert.Equal(25000, option.TotalPointsUsed);
            Assert.Equal(25000, option.PointsToPay);
        }

        [Fact]
        public async Task CapsCompanionCoverageAtVoucherValue_WhenCompanionFareIsHigher()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                [new VoucherDetail { VoucherNumber = "V-75000", MaxValue = "75000" }],
                eligibleCompanionPaxCount: 1,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 80000,
                totalAwardPointsNeededToPay: 160000);

            Assert.NotNull(result);
            var option = Assert.Single(result.Value.RedemptionOptions);

            Assert.Equal(75000, option.AppliedVouchers.Single().PointsUsed);
            Assert.Equal(75000, option.TotalPointsUsed);
            Assert.Equal(85000, option.PointsToPay);
        }

        [Fact]
        public async Task UsesMultipleVouchers_WhenCompanionPointsExceedSingleVoucherValue()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var vouchers = new List<VoucherDetail>
            {
                new() { VoucherNumber = "V-1000", MaxValue = "1000" },
                new() { VoucherNumber = "V-2000", MaxValue = "2000" },
            };

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                vouchers,
                eligibleCompanionPaxCount: 2,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 4000,
                totalAwardPointsNeededToPay: 8000);

            Assert.NotNull(result);

            var options = result.Value.RedemptionOptions.ToList();
            Assert.Equal(3, options.Count); // 1-voucher (x2) + 2-voucher (x1)

            var best = result.Value.BestOption;
            var bestOption = options.First(o => o.OptionId == best.OptionId);
            Assert.Equal(2, bestOption.AppliedVouchers.Count());
            Assert.Equal(1000, bestOption.AppliedVouchers.First(v => v.VoucherId == "V-1000").PointsUsed);
            Assert.Equal(2000, bestOption.AppliedVouchers.First(v => v.VoucherId == "V-2000").PointsUsed);
        }

        [Fact]
        public async Task CapsToTwoVouchers_WhenMoreThanTwoVouchersAreReturnedFromCustomerProfile()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var vouchers = new List<VoucherDetail>
            {
                new() { VoucherNumber = "V-1000", MaxValue = "1000" },
                new() { VoucherNumber = "V-2000", MaxValue = "2000" },
                new() { VoucherNumber = "V-3000", MaxValue = "3000" },
            };

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                vouchers,
                eligibleCompanionPaxCount: 3,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 2500,
                totalAwardPointsNeededToPay: 5000);

            Assert.NotNull(result);

            // Still returns all 1-voucher options (so UI can choose), but only one 2-voucher option (max 2 vouchers).
            var options = result.Value.RedemptionOptions.ToList();
            Assert.Equal(4, options.Count);
            Assert.Contains(options, option => option.AppliedVouchers.Count() == 2);
        }

        [Fact]
        public async Task ReturnsUpgradeOptions_AndUsesVoucherPointsToSelectBestOption()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");
            var vouchers = new List<VoucherDetail>
            {
                new() { VoucherNumber = "V-75000-A", MaxValue = "75000" },
                new() { VoucherNumber = "V-75000-B", MaxValue = "75000" },
            };

            var result = await service.GetVoucherRedemptionOffersForUpgrade(offerPriceResponse, vouchers, eligiblePaxCountExcludingInfants: 2, totalUpgradePoints: 138000, totalAwardPointsNeededToPay: 138000, totalBaselinePoints: 0);

            Assert.NotNull(result);
            var options = result.Value.RedemptionOptions.ToList();
            Assert.All(options, option => Assert.Equal("UPGRADE", option.RedemptionType));
            Assert.All(options, option => Assert.Equal(2, option.CoveredPassengers));
            var bestOption = options.Single(option => option.OptionId == result.Value.BestOption.OptionId);
            Assert.Equal(2, bestOption.AppliedVouchers.Count());
            Assert.Equal(138000, bestOption.AppliedVouchers.Sum(voucher => voucher.PointsUsed));
        }

        [Fact]
        public async Task ReturnsUpgradeOptions_WithOneCoveredPassengerForSingleAdult()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");
            var vouchers = new List<VoucherDetail>
            {
                new() { VoucherNumber = "V-43000", MaxValue = "43000" },
            };

            var result = await service.GetVoucherRedemptionOffersForUpgrade(
                offerPriceResponse,
                vouchers,
                eligiblePaxCountExcludingInfants: 1,
                totalUpgradePoints: 43000,
                totalAwardPointsNeededToPay: 43000,
                totalBaselinePoints: 0);

            Assert.NotNull(result);
            Assert.All(result.Value.RedemptionOptions, option => Assert.Equal(1, option.CoveredPassengers));
        }

        [Fact]
        public async Task UsesOnlyTheUpgradePointsDifference_WhenVoucherValueExceedsIt()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var result = await service.GetVoucherRedemptionOffersForUpgrade(
                offerPriceResponse,
                [new VoucherDetail { VoucherNumber = "V-75000", MaxValue = "75000" }],
                eligiblePaxCountExcludingInfants: 1,
                totalUpgradePoints: 15000,
                totalAwardPointsNeededToPay: 165000,
                totalBaselinePoints: 0);

            var option = Assert.Single(result!.Value.RedemptionOptions);
            var voucher = Assert.Single(option.AppliedVouchers);
            Assert.Equal(15000, voucher.PointsUsed);
            Assert.Equal(150000, option.PointsToPay);
            Assert.Equal(15000, option.TotalPointsUsed);
        }

        [Fact]
        public async Task ReturnsBaselinePoints_ForUpgradeRedemption()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var result = await service.GetVoucherRedemptionOffersForUpgrade(
                offerPriceResponse,
                [new VoucherDetail { VoucherNumber = "V-75000", MaxValue = "75000" }],
                eligiblePaxCountExcludingInfants: 2,
                totalUpgradePoints: 40000,
                totalAwardPointsNeededToPay: 120000,
                totalBaselinePoints: 80000);

            var option = Assert.Single(result!.Value.RedemptionOptions);
            Assert.Equal(80000, option.BaselinePoints);
        }

        [Fact]
        public async Task SelectsBestUpgradeOption_UsingAtMostTwoVouchers_WhenMoreVouchersAreAvailable()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");
            var vouchers = Enumerable.Range(1, 11)
                .Select(index => new VoucherDetail { VoucherNumber = $"V-75000-{index:D2}", MaxValue = "75000" })
                .ToList();

            var result = await service.GetVoucherRedemptionOffersForUpgrade(
                offerPriceResponse,
                vouchers,
                eligiblePaxCountExcludingInfants: 2,
                totalUpgradePoints: 140000,
                totalAwardPointsNeededToPay: 140000,
                totalBaselinePoints: 0,
                maxVouchersToUse: 2);

            Assert.NotNull(result);
            Assert.All(result.Value.RedemptionOptions, option => Assert.InRange(option.AppliedVouchers.Count(), 1, 2));

            var option = result.Value.RedemptionOptions.Single(option => option.OptionId == result.Value.BestOption.OptionId);
            Assert.Equal("UPGRADE", option.RedemptionType);
            Assert.Equal(2, option.AppliedVouchers.Count());
            Assert.Equal(140000, option.AppliedVouchers.Sum(voucher => voucher.PointsUsed));
            Assert.Equal(0, option.PointsToPay);
        }

        [Fact]
        public async Task DoesNotApplyASecondVoucher_WhenTheFirstVoucherFullyCoversTheUpgrade()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");
            var vouchers = new List<VoucherDetail>
            {
                new() { VoucherNumber = "V-75000-A", MaxValue = "75000" },
                new() { VoucherNumber = "V-75000-B", MaxValue = "75000" },
            };

            var result = await service.GetVoucherRedemptionOffersForUpgrade(
                offerPriceResponse,
                vouchers,
                eligiblePaxCountExcludingInfants: 2,
                totalUpgradePoints: 50000,
                totalAwardPointsNeededToPay: 50000,
                totalBaselinePoints: 0,
                maxVouchersToUse: 2);

            Assert.NotNull(result);
            Assert.All(result.Value.RedemptionOptions, option => Assert.All(option.AppliedVouchers, voucher => Assert.True(voucher.PointsUsed > 0)));

            var bestOption = result.Value.RedemptionOptions.Single(option => option.OptionId == result.Value.BestOption.OptionId);
            var appliedVoucher = Assert.Single(bestOption.AppliedVouchers);
            Assert.Equal(50000, appliedVoucher.PointsUsed);
            Assert.Equal(0, bestOption.PointsToPay);
        }

        [Fact]
        public async Task UsesOnlyOneVoucherForOneEligibleCompanionSeat()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");
            var vouchers = new List<VoucherDetail>
            {
                new() { VoucherNumber = "V-75000-A", MaxValue = "75000" },
                new() { VoucherNumber = "V-75000-B", MaxValue = "75000" },
            };

            var result = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                vouchers,
                eligibleCompanionPaxCount: 1,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 175000,
                totalAwardPointsNeededToPay: 350000,
                maxVouchersToUse: 2);

            Assert.NotNull(result);
            var bestOption = result.Value.RedemptionOptions.Single(option => option.OptionId == result.Value.BestOption.OptionId);

            Assert.Equal("COMPANION", bestOption.RedemptionType);
            Assert.Single(bestOption.AppliedVouchers);
            Assert.Equal(75000, bestOption.AppliedVouchers.Sum(voucher => voucher.PointsUsed));
            Assert.Equal(1, bestOption.CoveredPassengers);
            Assert.Equal(275000, bestOption.PointsToPay);
        }

        [Fact]
        public async Task SelectsCompanionAsBestValue_WhenCompanionUsesMoreVoucherPointsThanUpgrade()
        {
            var flightOfferResponse = new FlightOfferResponse
            {
                Offers =
                [
                    new OfferDetails
                    {
                        OfferItems =
                        [
                            new OfferItem
                            {
                                Services =
                                [
                                    new OfferService
                                    {
                                        FareDetails = new FareDetails
                                        {
                                            FareSegments =
                                            [
                                                new FareSegment
                                                {
                                                    CurrentCabinSalePrice = new Models.FlightOffer.Price { PointsAmount = new PointsAmount { PointsCount = 285000 } },
                                                    BaselineCabinSalePrice = new Models.FlightOffer.Price { PointsAmount = new PointsAmount { PointsCount = 0 } }
                                                }
                                            ]
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                ]
            };

            var upgradeEligible = VoucherUpgradePointsCalculator.TryCalculateTotalUpgradePoints(
                [flightOfferResponse],
                [new VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary.Flight { FareFamilyType = "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY" }],
                eligiblePassengerCount: 2,
                out _);

            var service = new VoucherRedemptionService();
            var companionResult = await service.GetVoucherRedemptionOffersForCompanion(
                flightOfferResponse,
                [new VoucherDetail { VoucherNumber = "V-75000", MaxValue = "75000" }],
                eligibleCompanionPaxCount: 1,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 30000,
                totalAwardPointsNeededToPay: 60000);

            Assert.True(upgradeEligible);
            Assert.NotNull(companionResult);

            var bestOption = service.SelectBestRedemptionOption(companionResult.Value.RedemptionOptions);
            Assert.NotNull(bestOption);
            Assert.Equal("COMPANION", bestOption.RedemptionType);
        }

        [Fact]
        public void SelectBestRedemptionOption_PrefersHighestVoucherPointsUtilisation()
        {
            var service = new VoucherRedemptionService();
            var bestOption = service.SelectBestRedemptionOption(
            [
                new RedemptionOption { OptionId = 1, RedemptionType = "UPGRADE", PointsToPay = 0, AppliedVouchers = [new AppliedVoucher { PointsUsed = 120000 }] },
                new RedemptionOption { OptionId = 2, RedemptionType = "COMPANION", PointsToPay = 0, AppliedVouchers = [new AppliedVoucher { PointsUsed = 150000 }] },
            ]);

            Assert.NotNull(bestOption);
            Assert.Equal(2, bestOption.OptionId);
            Assert.Equal("COMPANION", bestOption.RedemptionType);
        }

        [Fact]
        public void SelectBestRedemptionOption_PrefersUpgradeWhenCompanionAndUpgradeAreTied()
        {
            var service = new VoucherRedemptionService();
            var bestOption = service.SelectBestRedemptionOption(
            [
                new RedemptionOption
                {
                    OptionId = 1,
                    RedemptionType = "COMPANION",
                    PointsToPay = 550000,
                    AppliedVouchers = [new AppliedVoucher { PointsUsed = 75000 }, new AppliedVoucher { PointsUsed = 75000 }]
                },
                new RedemptionOption
                {
                    OptionId = 2,
                    RedemptionType = "UPGRADE",
                    PointsToPay = 0,
                    AppliedVouchers = [new AppliedVoucher { PointsUsed = 75000 }, new AppliedVoucher { PointsUsed = 75000 }]
                },
            ]);

            Assert.NotNull(bestOption);
            Assert.Equal(2, bestOption.OptionId);
            Assert.Equal("UPGRADE", bestOption.RedemptionType);
        }

        [Fact]
        public void SelectBestRedemptionOption_PrefersCompanionWhenItUsesMoreVoucherPointsThanUpgrade()
        {
            var service = new VoucherRedemptionService();
            var bestOption = service.SelectBestRedemptionOption(
            [
                new RedemptionOption
                {
                    OptionId = 1,
                    RedemptionType = "COMPANION",
                    PointsToPay = 0,
                    AppliedVouchers = [new AppliedVoucher { PointsUsed = 50000 }]
                },
                new RedemptionOption
                {
                    OptionId = 2,
                    RedemptionType = "UPGRADE",
                    PointsToPay = 0,
                    AppliedVouchers = [new AppliedVoucher { PointsUsed = 40000 }]
                },
            ]);

            Assert.NotNull(bestOption);
            Assert.Equal(1, bestOption.OptionId);
            Assert.Equal("COMPANION", bestOption.RedemptionType);
        }

        [Fact]
        public void SelectBestRedemptionOption_PrefersLowerPointsToPayBeforeUpgradeTieBreaker()
        {
            var service = new VoucherRedemptionService();
            var bestOption = service.SelectBestRedemptionOption(
            [
                new RedemptionOption
                {
                    OptionId = 1,
                    RedemptionType = "COMPANION",
                    PointsToPay = 10000,
                    AppliedVouchers = [new AppliedVoucher { PointsUsed = 40000 }]
                },
                new RedemptionOption
                {
                    OptionId = 2,
                    RedemptionType = "UPGRADE",
                    PointsToPay = 20000,
                    AppliedVouchers = [new AppliedVoucher { PointsUsed = 40000 }]
                },
            ]);

            Assert.NotNull(bestOption);
            Assert.Equal(1, bestOption.OptionId);
            Assert.Equal("COMPANION", bestOption.RedemptionType);
        }

        [Fact]
        public async Task SelectBestRedemptionOption_CombinesCompanionAndUpgradeOptions()
        {
            var service = new VoucherRedemptionService();
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");
            var vouchers = new List<VoucherDetail>
            {
                new() { VoucherNumber = "V-75000-A", MaxValue = "75000" },
                new() { VoucherNumber = "V-75000-B", MaxValue = "75000" },
            };

            var upgradeResult = await service.GetVoucherRedemptionOffersForUpgrade(
                offerPriceResponse,
                vouchers,
                eligiblePaxCountExcludingInfants: 2,
                totalUpgradePoints: 138000,
                totalAwardPointsNeededToPay: 138000,
                totalBaselinePoints: 0);
            var companionResult = await service.GetVoucherRedemptionOffersForCompanion(
                offerPriceResponse,
                vouchers,
                eligibleCompanionPaxCount: 2,
                bookingJourneyOfferDetails: null,
                totalPointsForCompanionToPay: 60000,
                totalAwardPointsNeededToPay: 120000);

            Assert.NotNull(upgradeResult);
            Assert.NotNull(companionResult);

            var allOptions = upgradeResult.Value.RedemptionOptions
                .Concat(companionResult.Value.RedemptionOptions)
                .Select((option, index) => new RedemptionOption
                {
                    OptionId = index + 1,
                    OptionName = option.OptionName,
                    RedemptionType = option.RedemptionType,
                    AppliedVouchers = option.AppliedVouchers,
                    CoveredPassengers = option.CoveredPassengers,
                    PointsToPay = option.PointsToPay,
                    CashToPay = option.CashToPay
                })
                .ToArray();

            var bestOption = service.SelectBestRedemptionOption(allOptions);

            Assert.Contains(allOptions, option => option.RedemptionType == "UPGRADE");
            Assert.Contains(allOptions, option => option.RedemptionType == "COMPANION");
            Assert.NotNull(bestOption);
            Assert.Equal("UPGRADE", bestOption.RedemptionType);
            Assert.Equal(138000, allOptions.Single(option => option.OptionId == bestOption.OptionId).AppliedVouchers.Sum(voucher => voucher.PointsUsed));
        }
    }
}
