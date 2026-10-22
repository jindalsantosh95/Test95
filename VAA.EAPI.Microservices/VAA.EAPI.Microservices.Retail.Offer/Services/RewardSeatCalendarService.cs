using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Offer.Domain.RewardSeatAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Interfaces;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita;

namespace VAA.EAPI.Microservices.Retail.Offer.Services
{

    public sealed class RewardSeatCalendarService : IRewardSeatCalendarService
    {
        private static readonly string[] FareFamilyRows =
        [
            "REWARD-SEAT-FINDER-ECONOMY",
            "REWARD-SEAT-FINDER-PREMIUM",
            "REWARD-SEAT-FINDER-UPPER"
        ];

        private static readonly string[] RequiredFareFamilies =
        [
            "AWARD-ECONOMY",
            "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY",
            "AWARD-BUSINESS-FIRST"
        ];

        private readonly IFareFamily _fareFamily;
        private readonly IItaService _itaService;

        public RewardSeatCalendarService(IFareFamily fareFamily, IItaService itaService)
        {
            _fareFamily = fareFamily ?? throw new ArgumentNullException(nameof(fareFamily));
            _itaService = itaService ?? throw new ArgumentNullException(nameof(itaService));
        }

        public async Task<RewardSeatCalendarResponse> SearchAsync(
            RewardSeatCalendarRequest request,
            TimeSpan remainingDeadline,
            CancellationToken cancellationToken)
        {
            ValidateRequest(request);
            if (remainingDeadline <= TimeSpan.Zero)
                throw new TimeoutException("The availability deadline has expired.");
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(remainingDeadline < TimeSpan.FromSeconds(30) ? remainingDeadline : TimeSpan.FromSeconds(30));

            var mappingVersion = ConfigReader.Get(Constants.Environment.RewardSeatAvailabilityFareFamilyMappingVersion).Trim();
            if (mappingVersion.Length == 0)
                throw new InvalidOperationException("Reward availability version settings are invalid.");

            var rows = await _fareFamily.GetFareFamilies(FareFamilyRows, deadline.Token);
            var fareFamilies = new List<string>(3);
            for (var index = 0; index < FareFamilyRows.Length; index++)
            {
                deadline.Token.ThrowIfCancellationRequested();
                var family = rows.TryGetValue(FareFamilyRows[index], out var value) ? value : null;
                if (!string.Equals(family, RequiredFareFamilies[index], StringComparison.Ordinal))
                    throw new InvalidOperationException("Reward fare-family mapping is missing or unsupported.");
                fareFamilies.Add(family);
            }

            var policy = RewardSeatCalendarPolicy.Load();
            var bookingCodes = new[]
            {
                ConfigReader.Get(Constants.Environment.RewardSeatEconomyBookingCode(policy.PermittedCarrier)).Trim(),
                ConfigReader.Get(Constants.Environment.RewardSeatPremiumBookingCode(policy.PermittedCarrier)).Trim(),
                ConfigReader.Get(Constants.Environment.RewardSeatUpperBookingCode(policy.PermittedCarrier)).Trim()
            };
            if (bookingCodes.Any(code => code.Length is < 1 or > 2) ||
                bookingCodes.Distinct(StringComparer.OrdinalIgnoreCase).Count() != 3)
                throw new InvalidOperationException("Reward booking-code mappings are invalid.");

            var search = RewardSeatCalendarItaRequestMapper.Map(request, policy, fareFamilies);
            var result = await _itaService.GetOffers(search, deadline.Token);
            deadline.Token.ThrowIfCancellationRequested();
            return RewardSeatCalendarItaResponseMapper.Map(result, request, policy, fareFamilies, bookingCodes, mappingVersion);
        }

        private static void ValidateRequest(RewardSeatCalendarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (!IsCode(request.Origin, 3) || !IsCode(request.Destination, 3) ||
                request.Origin.Is(request.Destination) ||
                request.AnchorDate == default || request.IntervalStart == default || request.IntervalEnd == default ||
                request.IntervalStart > request.AnchorDate || request.IntervalEnd < request.AnchorDate ||
                request.IntervalEnd < request.IntervalStart || request.IntervalEnd.DayNumber - request.IntervalStart.DayNumber > 31)
                throw new ArgumentException("Reward calendar request is invalid.", nameof(request));
        }

        private static bool IsCode(string value, int length) =>
            value is not null && value.Length == length && value.All(char.IsAsciiLetterUpper);

    }
}
