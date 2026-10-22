using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Offer.Domain.RewardSeatAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita
{

    public static class RewardSeatCalendarItaResponseMapper
    {
        private static readonly string[] Cabins = ["awardEconomy", "awardPremium", "awardUpperClass"];

        public static RewardSeatCalendarResponse Map(
            result source,
            RewardSeatCalendarRequest request,
            RewardSeatCalendarPolicy policy,
            IEnumerable<string> fareFamilies,
            IEnumerable<string> bookingCodes,
            string mappingVersion)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(policy);

            if (source.messages?.error?.Length > 0 || source.messages?.warning?.Length > 0)
                throw new InvalidOperationException("ITA calendar search returned an error.");

            if ((source.error?.code ?? string.Empty).Is("OfferSearchNoOffers"))
                return new RewardSeatCalendarResponse
                {
                    Candidates = [],
                    MarketPolicyVersion = policy.MarketPolicyVersion,
                    FareFamilyMappingVersion = mappingVersion,
                    Complete = true
                };

            if (source.error is not null)
                throw new InvalidOperationException("ITA calendar search returned an error.");

            ArgumentNullException.ThrowIfNull(fareFamilies);
            ArgumentNullException.ThrowIfNull(bookingCodes);
            var families = fareFamilies.Take(4).ToArray();
            var codes = bookingCodes.Take(4).ToArray();
            if (families.Length != Cabins.Length || codes.Length != Cabins.Length)
                throw new InvalidOperationException("Reward fare-family or booking-code mapping is incomplete.");
            var passengerTypes = policy.PassengerTypes?.ToArray()
                ?? throw new InvalidOperationException("Requested passenger types are missing.");

            var availability = source.Items?.OfType<resultAvailSummary>().SingleOrDefault()
                ?? throw new InvalidOperationException("ITA availability summary is missing.");
            var pricing = source.Items?.OfType<resultSliceBySliceSummary>().SingleOrDefault()
                ?? throw new InvalidOperationException("ITA pricing summary is missing.");

            if (pricing.pages is not { count: 1, current: 1 })
                throw new InvalidOperationException("ITA calendar result has incomplete paging metadata or unconsumed pages.");

            var columns = pricing.column ?? throw new InvalidOperationException("ITA cabin columns are missing.");
            if (columns.Length != 3 || columns.Select(column => column.cabin).Distinct(StringComparer.Ordinal).Count() != 3 ||
                !families.All(family => columns.Any(column => column.cabin == family)))
                throw new InvalidOperationException("ITA cabin columns do not match the configured reward families.");

            var inventory = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var solution in availability.solution ?? [])
            {
                if (solution.itinerary is not { Length: > 0 })
                    throw new InvalidOperationException("ITA availability itinerary metadata is missing.");
                if (solution.itinerary.Length > 1)
                    continue;
                foreach (var slice in solution.itinerary)
                {
                    if (slice.segment is null || slice.segment.Length == 0)
                        throw new InvalidOperationException("ITA availability segment metadata is missing.");
                    if (slice.segment.Length > 1)
                        continue;

                    var segment = slice.segment[0];
                    if (segment.leg is null || segment.leg.Length == 0)
                        throw new InvalidOperationException("ITA availability leg metadata is missing.");
                    if (segment.leg.Length > 1 || segment.stop is { Length: > 0 })
                        continue;
                    if (!IsRequestedFlight(segment.origin, segment.destination, segment.flight?.carrier, request, policy))
                        continue;

                    var flight = FlightKey(segment.origin, segment.destination, segment.departure?.date,
                        segment.departure?.time, segment.flight?.carrier, segment.flight?.number,
                        segment.operationalFlight?.carrier, segment.operationalFlight?.number);
                    if (flight is null)
                        throw new InvalidOperationException("ITA availability flight identity is incomplete.");

                    for (var cabinIndex = 0; cabinIndex < Cabins.Length; cabinIndex++)
                    {
                        var matchingCounts = (segment.availability ?? [])
                            .SelectMany(value => value.bookingCodeCount ?? [])
                            .Where(value => value.code.Is(codes[cabinIndex]))
                            .ToArray();
                        if (matchingCounts.Any(value => !value.countSpecified))
                            throw new InvalidOperationException("ITA availability booking-code count is missing.");
                        var counts = matchingCounts.Select(value => value.count)
                            .Distinct()
                            .ToArray();
                        if (counts.Length > 1)
                            throw new InvalidOperationException("ITA availability contains conflicting booking-code inventory.");
                        if (counts.Length == 0)
                            continue;
                        var count = counts[0];
                        if (count < 0)
                            throw new InvalidOperationException("ITA returned negative reward inventory.");
                        var key = flight.Value.Identity + "|" + Cabins[cabinIndex];
                        if (inventory.TryGetValue(key, out var existing) && existing != count)
                            throw new InvalidOperationException("ITA availability contains conflicting flight inventory.");
                        inventory[key] = count;
                    }
                }
            }

            var candidates = new List<RewardSeatCalendarCandidate>();
            foreach (var row in pricing.row ?? [])
            {
                if (row.cell is null || row.cell.Length != columns.Length)
                    throw new InvalidOperationException("ITA pricing row does not preserve cabin columns.");
                if (row.slice?.segment is not { Length: > 0 })
                    throw new InvalidOperationException("ITA pricing segment metadata is missing.");
                if (row.slice.segment.Length > 1)
                    continue;

                var segment = row.slice.segment[0];
                if (segment.leg is null || segment.leg.Length == 0)
                    throw new InvalidOperationException("ITA pricing direct-flight metadata is incomplete.");
                if (segment.leg.Length > 1)
                    continue;
                if (!segment.stopCountSpecified || segment.stopCount < 0)
                    throw new InvalidOperationException("ITA pricing direct-flight metadata is incomplete.");
                if (segment.stopCount > 0)
                    continue;
                if (!IsRequestedFlight(segment.origin, segment.destination, segment.flight?.carrier, request, policy))
                    continue;

                var flight = FlightKey(segment.origin, segment.destination, segment.departure?.date,
                    segment.departure?.time, segment.flight?.carrier, segment.flight?.number,
                    segment.operationalFlight?.carrier, segment.operationalFlight?.number);
                if (flight is null)
                    throw new InvalidOperationException("ITA pricing flight identity is incomplete.");
                if (flight.Value.Date < request.IntervalStart || flight.Value.Date > request.IntervalEnd)
                    continue;

                for (var columnIndex = 0; columnIndex < columns.Length; columnIndex++)
                {
                    var priced = row.cell[columnIndex]?.solution;
                    if (priced is null)
                        continue;

                    var cabinIndex = Array.IndexOf(families, columns[columnIndex].cabin);
                    var cabin = Cabins[cabinIndex];
                    var key = flight.Value.Identity + "|" + cabin;
                    if (!inventory.TryGetValue(key, out var count))
                        throw new InvalidOperationException("ITA priced flight has no matching inventory.");
                    if (count == 0)
                        continue;
                    if (!priced.awardPointsTotalSpecified || priced.awardPointsTotal < 0 ||
                        priced.displayTotal is null || !priced.displayTotal.amountSpecified || priced.displayTotal.amount < 0 ||
                        !priced.displayTotal.currency.Is(policy.Currency))
                        throw new InvalidOperationException("ITA reward price is incomplete or has an unexpected currency.");

                    var fares = (priced.pricing ?? []).SelectMany(value => value.fare ?? []).ToArray();
                    if (fares.Length == 0 || fares.Any(fare => fare is null ||
                        fare.extendedFareCode.IsNullOrWhiteSpace() ||
                        fare.ptc is not { Length: > 0 } || fare.ptc.Any(code => code.IsNullOrWhiteSpace()) ||
                        fare.bookingInfo is not { Length: > 0 } ||
                        fare.bookingInfo.Any(info => info is null || info.bookingCode.IsNullOrWhiteSpace())))
                        throw new InvalidOperationException("ITA reward fare identity is incomplete.");
                    if (fares.Any(fare => fare.bookingInfo.Any(info => !info.bookingCode.Is(codes[cabinIndex]))))
                        throw new InvalidOperationException("ITA reward fare booking code does not match cabin inventory.");
                    ValidatePassengerCoverage(priced.pricing ?? [], fares, passengerTypes);
                    var designators = fares.Select(fare => fare.ticketDesignator?.Trim().ToUpperInvariant()).ToArray();
                    bool? saver = designators.Any(code => code is not null && Constants.Application.SaverFareTicketDesignators.Contains(code, StringComparer.Ordinal))
                        ? true
                        : designators.All(code => !code.IsNullOrWhiteSpace()) ? false : null;

                    // JSON gives each provider-controlled component an unambiguous boundary, including
                    // null versus a literal marker. Sort the normalized components before hashing.
                    var fareComponents = fares.Select(fare => Json.Serialize(new
                    {
                        PassengerTypes = fare.ptc.Select(code => code.Trim().ToUpperInvariant())
                                .OrderBy(code => code, StringComparer.Ordinal).ToArray(),
                        BookingCodes = fare.bookingInfo.Select(info => info.bookingCode.Trim().ToUpperInvariant())
                                .OrderBy(code => code, StringComparer.Ordinal).ToArray(),
                        FareCode = fare.extendedFareCode.Trim().ToUpperInvariant(),
                        TicketDesignator = fare.ticketDesignator.IsNullOrWhiteSpace()
                                ? null : fare.ticketDesignator.Trim().ToUpperInvariant()
                    }, ignoreNullValues: false))
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray();
                    var fareHash = Convert.ToHexString(SHA256.HashData(
                        Encoding.UTF8.GetBytes(Json.Serialize(fareComponents)))).ToLowerInvariant();
                    var fareIdentity = string.Join("|", policy.Market, policy.CountryIssued, policy.CountrySold,
                        policy.Currency, policy.SalesCity,
                        string.Join(",", passengerTypes.OrderBy(value => value, StringComparer.Ordinal)),
                        "v2:" + fareHash);
                    candidates.Add(new RewardSeatCalendarCandidate
                    {
                        Date = flight.Value.Date,
                        Cabin = cabin,
                        FlightIdentity = flight.Value.Identity,
                        FareIdentity = fareIdentity,
                        InventoryCount = Math.Min(9, count),
                        AwardPointsTotal = priced.awardPointsTotal,
                        CashTotal = priced.displayTotal.amount,
                        Currency = policy.Currency,
                        IsSaverAward = saver
                    });
                }
            }

            return new RewardSeatCalendarResponse
            {
                Candidates = candidates,
                MarketPolicyVersion = policy.MarketPolicyVersion,
                FareFamilyMappingVersion = mappingVersion,
                Complete = true
            };
        }

        private static (string Identity, DateOnly Date)? FlightKey(
            string origin, string destination, string date, string time, string carrier, int? number,
            string operatingCarrier, int? operatingNumber)
        {
            if (origin.IsNullOrWhiteSpace() || destination.IsNullOrWhiteSpace() ||
                time.IsNullOrWhiteSpace() || carrier.IsNullOrWhiteSpace() || number is null or <= 0 ||
                !DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var departure))
                return null;

            var hasOperatingCarrier = !operatingCarrier.IsNullOrWhiteSpace();
            var hasOperatingNumber = operatingNumber is > 0;
            if (hasOperatingCarrier != hasOperatingNumber || operatingNumber is <= 0)
                return null;

            var identity = string.Join("|", carrier.Trim().ToUpperInvariant(), number.Value.ToString(CultureInfo.InvariantCulture),
                (hasOperatingCarrier ? operatingCarrier : carrier).Trim().ToUpperInvariant(),
                (hasOperatingNumber ? operatingNumber!.Value : number.Value).ToString(CultureInfo.InvariantCulture),
                origin.Trim().ToUpperInvariant(), destination.Trim().ToUpperInvariant(),
                departure.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), time.Trim());
            return (identity, departure);
        }

        private static bool IsRequestedFlight(string origin, string destination, string carrier,
            RewardSeatCalendarRequest request, RewardSeatCalendarPolicy policy) =>
            origin.Is(request.Origin) &&
            destination.Is(request.Destination) &&
            carrier.Is(policy.PermittedCarrier);

        private static void ValidatePassengerCoverage(
            IEnumerable<resultSliceBySliceSummaryRowCellSolutionPricing> pricing,
            IEnumerable<resultSliceBySliceSummaryRowCellSolutionPricingFare> fares,
            IEnumerable<string> requestedTypes)
        {
            var requestedTypeArray = requestedTypes.ToArray();
            var requested = requestedTypeArray.GroupBy(value => value, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
            var passengers = pricing.SelectMany(value => value.passenger ?? []).ToArray();
            if (passengers.Length == 0 ||
                passengers.Any(passenger => passenger is null || passenger.id.IsNullOrWhiteSpace() ||
                    passenger.ptc is not { Length: > 0 } || passenger.ptc.Any(code => code.IsNullOrWhiteSpace()) ||
                    passenger.ptc.Distinct(StringComparer.Ordinal).Count() != 1))
                throw new InvalidOperationException("ITA reward pricing does not identify the complete passenger party.");
            var unique = passengers.GroupBy(passenger => passenger.id, StringComparer.Ordinal).ToArray();
            if (unique.Length != requestedTypeArray.Length || unique.Any(group =>
                    group.Select(passenger => passenger.ptc[0]).Distinct(StringComparer.Ordinal).Count() != 1))
                throw new InvalidOperationException("ITA reward pricing contains incomplete or conflicting passenger identities.");
            var returned = unique.Select(group => group.First().ptc[0])
                .GroupBy(value => value, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
            if (returned.Count != requested.Count || requested.Any(pair =>
                    !returned.TryGetValue(pair.Key, out var count) || count != pair.Value))
                throw new InvalidOperationException("ITA reward pricing does not cover the requested passenger party.");
            var fareTypes = fares.SelectMany(fare => fare.ptc ?? []).ToHashSet(StringComparer.Ordinal);
            if (!fareTypes.SetEquals(requested.Keys))
                throw new InvalidOperationException("ITA reward fares do not match the requested passenger types.");
        }
    }
}
