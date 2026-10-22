using System;
using System.Linq;
using VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability;
using VAA.EAPI.GraphQL.Domain.Search.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{

    public sealed class RewardSeatAvailabilityWorkUnitPartitionerTests
    {
        [Fact]
        public void Create_UsesCalendarAnchorsTodayAndInclusiveIntervalBoundaries()
        {
            var units = RewardSeatAvailabilityWorkUnitPartitioner.Create(
                2026,
                RewardSeatMonth.November,
                new DateOnly(2026, 11, 12),
                20,
                6);

            Assert.Equal(
                new[]
                {
                    new DateOnly(2026, 11, 12),
                    new DateOnly(2026, 11, 20),
                    new DateOnly(2026, 11, 28),
                    new DateOnly(2026, 11, 30)
                },
                units.Select(unit => unit.AnchorDate));
            Assert.All(units, unit => Assert.Equal(new DateOnly(2026, 11, 12), unit.IntervalStart));
            Assert.All(units, unit => Assert.Equal(new DateOnly(2026, 11, 30), unit.IntervalEnd));
        }

        [Fact]
        public void Create_DeduplicatesAnchorsAtTheBeginningOfTheInterval()
        {
            var units = RewardSeatAvailabilityWorkUnitPartitioner.Create(
                2026,
                RewardSeatMonth.November,
                new DateOnly(2026, 11, 1),
                27,
                6).ToArray();

            Assert.Equal(4, units.Length);
            Assert.Equal(new DateOnly(2026, 11, 1), units[0].AnchorDate);
            Assert.Equal(new DateOnly(2026, 11, 10), units[1].AnchorDate);
        }

        [Fact]
        public void Create_RejectsMonthsOutsideTheInclusiveBookingHorizon()
        {
            Assert.Throws<ArgumentException>(() => RewardSeatAvailabilityWorkUnitPartitioner.Create(
                2026,
                RewardSeatMonth.December,
                new DateOnly(2026, 11, 1),
                10,
                6));
        }

        [Fact]
        public void Create_RejectsRequestsThatExceedTheConfiguredWorkUnitLimit()
        {
            Assert.Throws<InvalidOperationException>(() => RewardSeatAvailabilityWorkUnitPartitioner.Create(
                2026,
                RewardSeatMonth.November,
                new DateOnly(2026, 11, 1),
                30,
                3));
        }
    }
}
