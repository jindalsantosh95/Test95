using System.Globalization;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using BasketSearchOriginDestination = VAA.EAPI.GraphQL.Domain.Search.Models.Basket.SearchOriginDestination;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Services
{
    internal static class VoucherRedemptionEligibility
    {
        private const string InfantPassengerType = "INFANT";
        internal const string VoucherExpiresBeforeOutboundReasonCode = "VOUCHER_EXPIRES_BEFORE_OUTBOUND";

        internal static int GetEligiblePassengerCountExcludingInfants(IEnumerable<Fare>? fares)
        {
            if (fares == null)
            {
                return 0;
            }

            return fares
                .Where(f => f != null && !IsInfantPassengerType(f.PassengerType))
                .Sum(f => f.Quantity);
        }

        internal static int GetEligibleCompanionPassengerCountExcludingInfants(IEnumerable<Fare>? fares)
        {
            var eligible = GetEligiblePassengerCountExcludingInfants(fares);
            if (eligible <= 0)
            {
                return 0;
            }

            return eligible / 2;
        }

        internal static bool IsEvenEligiblePassengerCountExcludingInfants(IEnumerable<Fare>? fares)
        {
            var eligible = GetEligiblePassengerCountExcludingInfants(fares);
            return eligible > 0 && eligible % 2 == 0;
        }

        internal static IEnumerable<VoucherDetail> GetVouchersValidForOutboundTravel(
            IEnumerable<VoucherDetail>? vouchers,
            IEnumerable<BasketSearchOriginDestination>? searchOriginDestinations)
        {
            var availableVouchers = vouchers ?? Enumerable.Empty<VoucherDetail>();
            if (!TryGetOutboundTravelDate(searchOriginDestinations, out var outboundTravelDate))
            {
                return availableVouchers;
            }

            return availableVouchers
                .Where(voucher => DateOnly.FromDateTime(voucher.ExpiryDate) >= outboundTravelDate);
        }

        internal static VoucherEligibility? GetVoucherEligibility(
            IEnumerable<VoucherDetail>? vouchers,
            IEnumerable<BasketSearchOriginDestination>? searchOriginDestinations)
        {
            var availableVouchers = vouchers ?? Enumerable.Empty<VoucherDetail>();
            if (!availableVouchers.Any() || !TryGetOutboundTravelDate(searchOriginDestinations, out _))
            {
                return null;
            }

            return GetVouchersValidForOutboundTravel(availableVouchers, searchOriginDestinations).Any()
                ? new VoucherEligibility { Eligible = true }
                : new VoucherEligibility
                {
                    Eligible = false,
                    ReasonCode = VoucherExpiresBeforeOutboundReasonCode
                };
        }

        private static bool TryGetOutboundTravelDate(
            IEnumerable<BasketSearchOriginDestination>? searchOriginDestinations,
            out DateOnly outboundTravelDate)
        {
            outboundTravelDate = default;
            var hasSearchOriginDestination = false;
            foreach (var searchOriginDestination in searchOriginDestinations ?? Enumerable.Empty<BasketSearchOriginDestination>())
            {
                hasSearchOriginDestination = true;
                if (!DateTime.TryParse(searchOriginDestination?.DepartureDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var departureDateTime))
                {
                    return false;
                }

                var departureDate = DateOnly.FromDateTime(departureDateTime);
                if (outboundTravelDate == default || departureDate < outboundTravelDate)
                {
                    outboundTravelDate = departureDate;
                }
            }

            return hasSearchOriginDestination;
        }

        private static bool IsInfantPassengerType(string? passengerType)
        {
            if (string.IsNullOrWhiteSpace(passengerType))
            {
                return false;
            }

            return string.Equals(passengerType, InfantPassengerType, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(passengerType, PtcEnums.PtcTypeEnum.INF.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}

