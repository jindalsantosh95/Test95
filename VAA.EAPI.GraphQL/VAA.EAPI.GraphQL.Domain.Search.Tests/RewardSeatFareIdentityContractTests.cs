using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability;
using VAA.EAPI.GraphQL.Domain.Search.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{

    public sealed class RewardSeatFareIdentityContractTests
    {
        [Fact]
        public void CalendarContract_SerializesOnlyRouteAndDateCriteria()
        {
            var request = new RewardSeatCalendarContract
            {
                Origin = "LHR",
                Destination = "JFK",
                AnchorDate = new DateOnly(2027, 1, 10),
                IntervalStart = new DateOnly(2027, 1, 1),
                IntervalEnd = new DateOnly(2027, 1, 31)
            };

            var json = JObject.Parse(Json.Serialize(request));

            Assert.Equal(5, json.Properties().Count());
            Assert.Equal("LHR", json.Value<string>("origin"));
            Assert.Equal("JFK", json.Value<string>("destination"));
            Assert.Null(json["market"]);
            Assert.Null(json["passengerTypes"]);
            Assert.Null(json["permittedCarrier"]);
        }

        [Fact]
        public void HasExpectedIdentity_TreatsFareIdentityAsOpaque()
        {
            var request = new RewardSeatCalendarContract
            {
                Origin = "LHR",
                Destination = "JFK"
            };
            var candidate = new RewardSeatCalendarCandidate
            {
                Date = new DateOnly(2027, 1, 10),
                FlightIdentity = "VS|9|VS|9|LHR|JFK|2027-01-10|10:00",
                FareIdentity = "offer-owned-fare-id"
            };

            Assert.True(RewardSeatAvailabilityQueryHandler.HasExpectedIdentity(candidate, request));
            candidate.FareIdentity = string.Empty;
            Assert.False(RewardSeatAvailabilityQueryHandler.HasExpectedIdentity(candidate, request));
        }
    }
}
