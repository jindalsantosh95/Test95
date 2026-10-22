using System;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Xml.Serialization;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Offer.Domain.RewardSeatAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{

    public sealed class RewardSeatCalendarItaRequestMapperTests
    {
        [Fact]
        public void Map_FormatsProviderDateInTheInvariantCalendar()
        {
            OfferTestsCommon.CreateTestEnvironmentSettings();
            ConfigReader.Set("RewardSeatAvailability:ItaVersion", "161");
            var request = new RewardSeatCalendarRequest
            {
                Origin = "LHR",
                Destination = "JFK",
                AnchorDate = new DateOnly(2027, 1, 10)
            };
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("th-TH");
                var mapped = RewardSeatCalendarItaRequestMapper.Map(request, Policy(),
                    ["AWARD-ECONOMY", "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY", "AWARD-BUSINESS-FIRST"]);
                Assert.Equal("2027-01-10", mapped.inputs.slice[0].date);
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Fact]
        public void Map_UsesExistingProxyWithBothSummariesAndApprovedAwardControls()
        {
            OfferTestsCommon.CreateTestEnvironmentSettings();
            ConfigReader.Set("RewardSeatAvailability:ItaVersion", "161");
            var request = new RewardSeatCalendarRequest
            {
                Origin = "LHR",
                Destination = "JFK",
                AnchorDate = new DateOnly(2027, 1, 10),
                IntervalStart = new DateOnly(2027, 1, 1),
                IntervalEnd = new DateOnly(2027, 1, 31)
            };
            var families = new[] { "AWARD-ECONOMY", "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY", "AWARD-BUSINESS-FIRST" };

            var mapped = RewardSeatCalendarItaRequestMapper.Map(request, Policy(), families);

            Assert.Equal(searchName.calendar, mapped.name);
            Assert.Equal("161", mapped.version);
            Assert.Equal([searchSummarizer.availSummary, searchSummarizer.sliceBySliceSummary], mapped.summarizer);
            Assert.Equal(families, mapped.inputs.fareFamilyToPrice.Select(x => x.name));
            Assert.Equal(1, mapped.inputs.passengers);
            Assert.Equal(["ADT"], mapped.inputs.paxType.SelectMany(x => x.paxTypeCode));
            Assert.Equal("VS", Assert.Single(mapped.inputs.permittedCarrier));
            Assert.Equal("CINCO", mapped.inputs.awardType);
            Assert.Equal("ADT", mapped.inputs.awardRedeemerPTC);
            Assert.Equal("GBR", mapped.inputs.countryIssued);
            Assert.Equal("GBR", mapped.inputs.countrySold);
            Assert.Equal("GBP", mapped.inputs.currency);
            Assert.Equal("LON", mapped.inputs.salesCity);
            Assert.True(mapped.inputs.nonstopOnly);
            Assert.Null(mapped.inputs.accountCode);
            Assert.False(mapped.inputs.requirePrivateFares);
            using var writer = new StringWriter();
            new XmlSerializer(typeof(search)).Serialize(writer, mapped);
            var xml = writer.ToString();
            Assert.Contains("availSummary", xml);
            Assert.Contains("sliceBySliceSummary", xml);
            Assert.Contains("countryIssued=\"GBR\"", xml);
            Assert.Contains("countrySold=\"GBR\"", xml);
            Assert.Contains("AWARD-ECONOMY", xml);
        }

        [Fact]
        public void InternalCalendarContract_RoundTripsDatesThroughExistingJsonSerializer()
        {
            var source = new RewardSeatCalendarRequest
            {
                Origin = "LHR",
                Destination = "JFK",
                AnchorDate = new DateOnly(2027, 1, 10),
                IntervalStart = new DateOnly(2027, 1, 1),
                IntervalEnd = new DateOnly(2027, 1, 31)
            };
            var json = Json.Serialize(source);
            var roundTrip = Json.Deserialize<RewardSeatCalendarRequest>(json);
            Assert.Equal(source.AnchorDate, roundTrip.AnchorDate);
            Assert.Equal(source.IntervalEnd, roundTrip.IntervalEnd);
            Assert.DoesNotContain("passengerTypes", json);
            Assert.DoesNotContain("market", json);
            Assert.DoesNotContain("permittedCarrier", json);
        }

        private static RewardSeatCalendarPolicy Policy() => new()
        {
            MarketPolicyVersion = "v1",
            CountryIssued = "GBR",
            CountrySold = "GBR",
            Currency = "GBP",
            SalesCity = "LON"
        };
    }
}
