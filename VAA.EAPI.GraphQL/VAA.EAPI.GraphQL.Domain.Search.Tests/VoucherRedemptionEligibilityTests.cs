using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Search.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{
    public sealed class VoucherRedemptionEligibilityTests
    {
        [Fact]
        public void EligiblePassengers_ExcludesInfants_FromFareCalculationSummaryFares()
        {
            var fares = new List<Fare>
            {
                new() { PassengerType = "ADULT", Quantity = 2 },
                new() { PassengerType = "INFANT", Quantity = 1 },
            };

            var eligible = VoucherRedemptionEligibility.GetEligiblePassengerCountExcludingInfants(fares);
            var companions = VoucherRedemptionEligibility.GetEligibleCompanionPassengerCountExcludingInfants(fares);
            var isEven = VoucherRedemptionEligibility.IsEvenEligiblePassengerCountExcludingInfants(fares);

            Assert.Equal(2, eligible);
            Assert.Equal(1, companions);
            Assert.True(isEven);
        }

        [Fact]
        public void EligiblePassengers_ReturnsZero_WhenOnlyInfants()
        {
            var fares = new List<Fare>
            {
                new() { PassengerType = "INFANT", Quantity = 1 },
            };

            Assert.Equal(0, VoucherRedemptionEligibility.GetEligiblePassengerCountExcludingInfants(fares));
            Assert.Equal(0, VoucherRedemptionEligibility.GetEligibleCompanionPassengerCountExcludingInfants(fares));
            Assert.False(VoucherRedemptionEligibility.IsEvenEligiblePassengerCountExcludingInfants(fares));
        }

        [Fact]
        public void EligiblePassengers_DoesNotCreateCompanionEligibility_ForOneNonInfantPassenger()
        {
            var fares = new List<Fare>
            {
                new() { PassengerType = "ADULT", Quantity = 1 },
                new() { PassengerType = "INFANT", Quantity = 1 },
            };

            Assert.Equal(1, VoucherRedemptionEligibility.GetEligiblePassengerCountExcludingInfants(fares));
            Assert.Equal(0, VoucherRedemptionEligibility.GetEligibleCompanionPassengerCountExcludingInfants(fares));
        }

        [Fact]
        public void ValidVouchersForOutboundTravel_ReturnsNone_WhenOutboundIsAfterEveryVoucherExpiry()
        {
            var vouchers = new[]
            {
                CreateVoucher("expired-1", new System.DateTime(2026, 8, 10)),
                CreateVoucher("expired-2", new System.DateTime(2026, 8, 16))
            };

            var result = VoucherRedemptionEligibility.GetVouchersValidForOutboundTravel(
                vouchers,
                [new SearchOriginDestination { DepartureDate = "2026-08-17" }]);

            Assert.Empty(result);
        }

        [Fact]
        public void ValidVouchersForOutboundTravel_RetainsOnlyVoucherWhoseExpiryIncludesOutboundDate()
        {
            var vouchers = new[]
            {
                CreateVoucher("expired", new System.DateTime(2026, 8, 10)),
                CreateVoucher("valid", new System.DateTime(2026, 8, 20))
            };

            var result = VoucherRedemptionEligibility.GetVouchersValidForOutboundTravel(
                vouchers,
                [new SearchOriginDestination { DepartureDate = "2026-08-17" }]);

            Assert.Equal(["valid"], result.Select(voucher => voucher.VoucherNumber));
        }

        [Fact]
        public void ValidVouchersForOutboundTravel_FiltersExpiredVoucher_WhenOutboundIncludesTimeComponent()
        {
            var vouchers = new[]
            {
                CreateVoucher("expired", new System.DateTime(2026, 8, 16)),
                CreateVoucher("valid", new System.DateTime(2026, 8, 17))
            };

            var result = VoucherRedemptionEligibility.GetVouchersValidForOutboundTravel(
                vouchers,
                [new SearchOriginDestination { DepartureDate = "2026-08-17T00:00:00" }]);

            Assert.Equal(["valid"], result.Select(voucher => voucher.VoucherNumber));
        }

        [Fact]
        public void ValidVouchersForOutboundTravel_UsesOutboundDateAndIgnoresInboundDate()
        {
            var vouchers = new[] { CreateVoucher("valid", new System.DateTime(2026, 8, 17)) };

            var result = VoucherRedemptionEligibility.GetVouchersValidForOutboundTravel(
                vouchers,
                [
                    new SearchOriginDestination { DepartureDate = "2026-08-17" },
                    new SearchOriginDestination { DepartureDate = "2026-08-18" }
                ]);

            Assert.Equal(["valid"], result.Select(voucher => voucher.VoucherNumber));
        }

        [Fact]
        public void ValidVouchersForOutboundTravel_UsesEarliestDepartureDate_WhenSearchSlicesAreUnordered()
        {
            var vouchers = new[] { CreateVoucher("valid", new System.DateTime(2026, 8, 18)) };

            var result = VoucherRedemptionEligibility.GetVouchersValidForOutboundTravel(
                vouchers,
                [
                    new SearchOriginDestination { DepartureDate = "2026-08-25" },
                    new SearchOriginDestination { DepartureDate = "2026-08-17" }
                ]);

            Assert.Equal(["valid"], result.Select(voucher => voucher.VoucherNumber));
        }

        [Fact]
        public void ValidVouchersForOutboundTravel_PreservesExistingVoucherAvailability_WhenAnySearchDateIsMalformed()
        {
            var vouchers = new[] { CreateVoucher("existing", new System.DateTime(2026, 8, 10)) };

            var result = VoucherRedemptionEligibility.GetVouchersValidForOutboundTravel(
                vouchers,
                [
                    new SearchOriginDestination { DepartureDate = "2026-08-17" },
                    new SearchOriginDestination { DepartureDate = "not-a-date" }
                ]);

            Assert.Equal(["existing"], result.Select(voucher => voucher.VoucherNumber));
        }

        [Fact]
        public void ValidVouchersForOutboundTravel_PreservesExistingVoucherAvailability_WhenOutboundDateIsUnavailable()
        {
            var vouchers = new[] { CreateVoucher("existing", new System.DateTime(2026, 8, 10)) };

            var result = VoucherRedemptionEligibility.GetVouchersValidForOutboundTravel(vouchers, null);

            Assert.Equal(["existing"], result.Select(voucher => voucher.VoucherNumber));
        }

        [Fact]
        public void VoucherEligibility_ReturnsExpiryReason_WhenOutboundTravelIsAfterVoucherExpiry()
        {
            var today = System.DateTime.Today;
            var eligibility = VoucherRedemptionEligibility.GetVoucherEligibility(
                [CreateVoucher("voucher-1", today.AddDays(3))],
                [
                    new SearchOriginDestination { DepartureDate = today.AddDays(5).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) }
                ]);

            Assert.NotNull(eligibility);
            Assert.False(eligibility.Eligible);
            Assert.Equal("VOUCHER_EXPIRES_BEFORE_OUTBOUND", eligibility.ReasonCode);
        }

        [Fact]
        public void VoucherEligibility_IsEligible_WhenVoucherCoversOutboundTravel()
        {
            var today = System.DateTime.Today;
            var eligibility = VoucherRedemptionEligibility.GetVoucherEligibility(
                [CreateVoucher("voucher-1", today.AddMonths(6))],
                [
                    new SearchOriginDestination { DepartureDate = today.AddMonths(1).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) }
                ]);

            Assert.NotNull(eligibility);
            Assert.True(eligibility.Eligible);
            Assert.Null(eligibility.ReasonCode);
        }

        private static VoucherDetail CreateVoucher(string voucherNumber, System.DateTime expiryDate)
        {
            return new VoucherDetail
            {
                VoucherNumber = voucherNumber,
                ExpiryDate = expiryDate
            };
        }
    }
}

