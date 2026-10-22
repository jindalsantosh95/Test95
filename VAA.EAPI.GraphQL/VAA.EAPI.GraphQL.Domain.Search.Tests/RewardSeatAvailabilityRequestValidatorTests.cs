using System;
using System.Collections.Generic;
using VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability;
using VAA.EAPI.GraphQL.Domain.Search.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{

    public sealed class RewardSeatAvailabilityRequestValidatorTests
    {
        [Fact]
        public void Validate_AcceptsTheBoundedPublicRequest()
        {
            Assert.True(RewardSeatAvailabilityRequestValidator.Validate(CreateRequest()).IsValid);
        }

        [Fact]
        public void Validate_RejectsInconsistentMonthSelection()
        {
            Assert.False(RewardSeatAvailabilityRequestValidator.Validate(CreateRequest() with { Years = new[] { 2027 } }).IsValid);
        }

        private static RewardSeatAvailabilityRequest CreateRequest()
        {
            return new RewardSeatAvailabilityRequest
            {
                Slice = new RewardSeatSliceInput { Origin = "LHR", Destination = "JFK", Departure = new DateOnly(2026, 11, 1) },
                Years = new[] { 2026 },
                Months = new[] { RewardSeatMonth.November }
            };
        }
    }
}
