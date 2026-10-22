using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability;
using VAA.EAPI.GraphQL.Domain.Search.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{

    public sealed class RewardSeatAvailabilityMergerTests
    {
        private static readonly IReadOnlyDictionary<string, (int Daily, int Monthly)> Caps = new Dictionary<string, (int, int)>
        {
            ["awardEconomy"] = (9, 100),
            ["awardPremium"] = (9, 50),
            ["awardUpperClass"] = (9, 50)
        };

        [Fact]
        public void Merge_DeduplicatesFlightsAndKeepsTheWinningPriceAtomic()
        {
            var date = new DateOnly(2027, 1, 10);
            var economy = Candidate(date, "awardEconomy", "VS|1|LHR|JFK|2027-01-10|10:00", "E", 6, 6000, 243.39m, true);
            var premium = Candidate(date, "awardPremium", "VS|2|LHR|JFK|2027-01-10|11:00", "P", 4, 10500, 200m, false);
            var result = RewardSeatAvailabilityMerger.Merge([Unit(economy, premium), Unit(economy)],
                date, date.AddDays(1), RewardSeatMonth.January, 2027, "London Heathrow", "New York JFK", Caps);

            Assert.Equal(10, result.TotalAwardsSeatsForMonth);
            var days = result.PointsDays.ToArray();
            Assert.Equal(2, days.Length);
            Assert.Equal(6000, days[0].MinAwardPointsTotal);
            Assert.Equal(243.39m, days[0].MinPrice);
            Assert.Equal(6, days[0].Seats!.AwardEconomy!.CabinClassSeatCount);
            Assert.Equal(200m, days[0].Seats.AwardPremium!.Price);
            Assert.Equal(0, days[1].MinAwardPointsTotal);
            Assert.Null(days[1].MinPrice);
            Assert.Null(days[1].Seats);
        }

        [Fact]
        public void Merge_MatchesFlightInventoryUnderNonGregorianCurrentCulture()
        {
            var date = new DateOnly(2027, 1, 10);
            var candidate = Candidate(date, "awardEconomy", "VS|9|LHR|JFK|2027-01-10|10:00", "E", 3, 6000, 243.39m, true);
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("th-TH");

                var result = RewardSeatAvailabilityMerger.Merge([Unit(candidate)], date, date,
                    RewardSeatMonth.January, 2027, "LHR", "JFK", Caps);

                Assert.Equal(3, Assert.Single(result.PointsDays).Seats!.AwardEconomy!.CabinClassSeatCount);
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Fact]
        public void Merge_RejectsConflictingOverlappingSolutions()
        {
            var date = new DateOnly(2027, 1, 10);
            var first = Candidate(date, "awardEconomy", "VS|1", "E", 3, 6000, 250m, null);
            var second = Candidate(date, "awardEconomy", "VS|1", "E", 3, 6500, 250m, null);
            Assert.Throws<InvalidOperationException>(() => RewardSeatAvailabilityMerger.Merge(
                [Unit(first), Unit(second)], date, date, RewardSeatMonth.January, 2027, "LHR", "JFK", Caps));
        }

        [Fact]
        public void Merge_UsesOfferCurrencyWithoutGraphQlMarketConfiguration()
        {
            var date = new DateOnly(2027, 1, 10);
            var candidate = Candidate(date, "awardEconomy", "VS|1", "E", 3, 6000, 250m, null);
            candidate.Currency = "USD";

            var result = RewardSeatAvailabilityMerger.Merge([Unit(candidate)], date, date,
                RewardSeatMonth.January, 2027, "LHR", "JFK", Caps);

            var day = Assert.Single(result.PointsDays);
            Assert.Equal("USD", day.Currency);
            Assert.Equal("USD", day.Seats!.AwardEconomy!.Currency);
        }

        [Fact]
        public void Merge_RejectsMixedOfferCurrencies()
        {
            var date = new DateOnly(2027, 1, 10);
            var gbp = Candidate(date, "awardEconomy", "VS|1", "E", 3, 6000, 250m, null);
            var usd = Candidate(date, "awardPremium", "VS|2", "P", 3, 9000, 300m, null);
            usd.Currency = "USD";

            Assert.Throws<InvalidOperationException>(() => RewardSeatAvailabilityMerger.Merge(
                [Unit(gbp, usd)], date, date, RewardSeatMonth.January, 2027, "LHR", "JFK", Caps));
        }

        [Fact]
        public void Merge_AppliesDailyCapAndMonthlyCap()
        {
            var start = new DateOnly(2027, 1, 1);
            var options = Enumerable.Range(0, 12).Select(day =>
                Candidate(start.AddDays(day), "awardEconomy", "VS|" + day, "E", 9, 6000, 100m, null)).ToArray();
            var result = RewardSeatAvailabilityMerger.Merge([Unit(options)], start, start.AddDays(11),
                RewardSeatMonth.January, 2027, "LHR", "JFK", Caps);
            Assert.Equal(100, result.TotalAwardsSeatsForMonth);
            Assert.All(result.PointsDays, x => Assert.Equal("9+", x.Seats!.AwardEconomy!.CabinClassSeatCountString));
        }

        private static RewardSeatCalendarCandidate Candidate(DateOnly date, string cabin, string flight, string fare,
            int seats, int points, decimal cash, bool? saver) => new()
            {
                Date = date,
                Cabin = cabin,
                FlightIdentity = flight,
                FareIdentity = fare,
                InventoryCount = seats,
                AwardPointsTotal = points,
                CashTotal = cash,
                Currency = "GBP",
                IsSaverAward = saver
            };

        private static RewardSeatCalendarResult Unit(params RewardSeatCalendarCandidate[] candidates) => new()
        {
            Candidates = candidates,
            Complete = true,
            FareFamilyMappingVersion = "v1"
        };
    }
}
