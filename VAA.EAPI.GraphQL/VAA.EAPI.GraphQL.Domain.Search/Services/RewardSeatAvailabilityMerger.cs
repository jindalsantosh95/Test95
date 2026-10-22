using VAA.EAPI.GraphQL.Common.Extensions;
using System.Globalization;
using VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability;

namespace VAA.EAPI.GraphQL.Domain.Search.Services
{

    internal static class RewardSeatAvailabilityMerger
    {
        internal static RewardSeatAvailabilityMonthResponse Merge(
            IEnumerable<RewardSeatCalendarResult> units, DateOnly start, DateOnly end,
            RewardSeatMonth month, int year, string originName, string destinationName,
            IReadOnlyDictionary<string, (int Daily, int Monthly)> caps)
        {
            var byIdentity = new Dictionary<string, RewardSeatCalendarCandidate>(StringComparer.Ordinal);
            var flights = new Dictionary<string, int>(StringComparer.Ordinal);
            string? currency = null;
            foreach (var unit in units)
            {
                if (!unit.Complete || unit.Candidates is null) throw new InvalidOperationException("Incomplete reward calendar unit.");
                foreach (var candidate in unit.Candidates)
                {
                    if (candidate.Date < start || candidate.Date > end) continue;
                    if (!caps.ContainsKey(candidate.Cabin) || candidate.AwardPointsTotal < 0 || candidate.InventoryCount is < 1 or > 9 ||
                        candidate.CashTotal < 0 || candidate.Currency.IsNullOrWhiteSpace() ||
                        candidate.FlightIdentity.IsNullOrWhiteSpace() || candidate.FareIdentity.IsNullOrWhiteSpace())
                        throw new InvalidOperationException("Invalid reward calendar candidate.");
                    currency ??= candidate.Currency;
                    if (!string.Equals(candidate.Currency, currency, StringComparison.Ordinal))
                        throw new InvalidOperationException("Conflicting reward calendar currency.");

                    var key = string.Join('|', candidate.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), candidate.Cabin, candidate.FlightIdentity, candidate.FareIdentity);
                    if (byIdentity.TryGetValue(key, out var previous))
                    {
                        if (previous.InventoryCount != candidate.InventoryCount || previous.AwardPointsTotal != candidate.AwardPointsTotal ||
                            previous.CashTotal != candidate.CashTotal || previous.Currency != candidate.Currency || previous.IsSaverAward != candidate.IsSaverAward)
                            throw new InvalidOperationException("Conflicting reward calendar solution.");
                    }
                    else byIdentity.Add(key, candidate);

                    var flightKey = string.Join('|', candidate.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), candidate.Cabin, candidate.FlightIdentity);
                    if (flights.TryGetValue(flightKey, out var count) && count != candidate.InventoryCount)
                        throw new InvalidOperationException("Conflicting reward calendar inventory.");
                    flights[flightKey] = candidate.InventoryCount;
                }
            }

            var days = new List<RewardSeatAvailabilityDayResponse>();
            var monthly = caps.Keys.ToDictionary(cabin => cabin, _ => 0, StringComparer.Ordinal);
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                var winners = new Dictionary<string, RewardSeatCalendarCandidate>(StringComparer.Ordinal);
                var cabinResults = new Dictionary<string, RewardSeatCabinResponse>(StringComparer.Ordinal);
                foreach (var (cabin, cap) in caps)
                {
                    var options = byIdentity.Values.Where(x => x.Date == date && x.Cabin == cabin).ToArray();
                    if (options.Length == 0) continue;
                    var winner = Order(options).First();
                    var prefix = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "|" + cabin + "|";
                    var seats = Math.Min(cap.Daily, flights.Where(x => x.Key.StartsWith(prefix, StringComparison.Ordinal)).Sum(x => x.Value));
                    if (seats <= 0) continue;
                    monthly[cabin] += seats;
                    winners[cabin] = winner;
                    cabinResults[cabin] = new RewardSeatCabinResponse
                    {
                        CabinPointsValue = winner.AwardPointsTotal,
                        Price = winner.CashTotal,
                        Currency = winner.Currency,
                        IsSaverAward = winner.IsSaverAward,
                        CabinClassSeatCount = seats,
                        CabinClassSeatCountString = seats.ToString(CultureInfo.InvariantCulture) + (seats >= cap.Daily ? "+" : string.Empty)
                    };
                }
                var dayWinner = Order(winners.Values).FirstOrDefault();
                days.Add(new RewardSeatAvailabilityDayResponse
                {
                    Date = date,
                    MinAwardPointsTotal = dayWinner?.AwardPointsTotal ?? 0,
                    MinPrice = dayWinner?.CashTotal,
                    Currency = dayWinner?.Currency,
                    Seats = cabinResults.Count == 0 ? null : new RewardSeatAvailabilitySeatsResponse
                    {
                        AwardEconomy = cabinResults.GetValueOrDefault("awardEconomy"),
                        AwardPremium = cabinResults.GetValueOrDefault("awardPremium"),
                        AwardUpperClass = cabinResults.GetValueOrDefault("awardUpperClass")
                    }
                });
            }
            return new RewardSeatAvailabilityMonthResponse
            {
                PointsDays = days,
                Month = month,
                Year = year,
                OriginPrettyName = originName,
                DestinationPrettyName = destinationName,
                TotalAwardsSeatsForMonth = monthly.Sum(x => Math.Min(x.Value, caps[x.Key].Monthly))
            };
        }

        private static IOrderedEnumerable<RewardSeatCalendarCandidate> Order(IEnumerable<RewardSeatCalendarCandidate> source) =>
            source.OrderBy(x => x.AwardPointsTotal)
                  .ThenBy(x => x.CashTotal)
                  .ThenBy(x => x.IsSaverAward == true ? 0 : x.IsSaverAward == false ? 1 : 2)
                  .ThenBy(x => x.Cabin, StringComparer.Ordinal)
                  .ThenBy(x => x.FlightIdentity, StringComparer.Ordinal)
                  .ThenBy(x => x.FareIdentity, StringComparer.Ordinal);
    }
}
