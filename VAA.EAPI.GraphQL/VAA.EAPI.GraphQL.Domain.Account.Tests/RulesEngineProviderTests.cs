using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class RulesEngineProviderTests
    {
        [Fact]
        public void MapFlightSegment_MapsPdmFlightAndAgentDetails()
        {
            var pdmResponse = new VSRetroFlownDetail
            {
                DepartureAirport = "LAX",
                ArrivalAirport = "IAH",
                OperatingAirlineCode = "DL",
                MarketingAirlineCode = "KL",
                OperatingFlightNumber = "1525",
                MarketingFlightNumber = "5378",
                DepartureDate = new DateTime(2024, 1, 1),
                FlownCabinClass = "J",
                OriginalBookedClass = "V",
                FlownUpgradedClass = "W",
                VAMarketingBookingClass = "V",
                SalesLocation = "US",
                IataAgentNumber = "9126901",
                FareBasis = "JBUSINESS"
            };

            var result = RulesEngineProvider<object>.MapFlightSegment(pdmResponse, "2458");

            Assert.Equal("J", result.ClassOfService);
            Assert.Equal("V", result.OperatingBookingClass);
            Assert.Equal("W", result.FlownClass);
            Assert.Equal("US", result.SalesLocation);
            Assert.Equal("9126901", result.AgentIataNumber);
            Assert.Equal("JBUSINESS", result.FareBasisCode);
            Assert.Equal("2024-01-01", result.ScheduledDepartureTime);
            Assert.Equal(2458, result.SegmentDistance);
        }

        [Fact]
        public void MapFlightSegment_UsesBlankValuesForMissingPdmDetails()
        {
            var result = RulesEngineProvider<object>.MapFlightSegment(new VSRetroFlownDetail(), "0");

            Assert.Equal(string.Empty, result.ClassOfService);
            Assert.Equal(string.Empty, result.CabinClass);
            Assert.Equal(string.Empty, result.OperatingBookingClass);
            Assert.Equal(string.Empty, result.FlownClass);
            Assert.Equal(string.Empty, result.ScheduledDepartureTime);
            Assert.Equal(string.Empty, result.SalesLocation);
            Assert.Equal(string.Empty, result.AgentIataNumber);
            Assert.Equal(string.Empty, result.FareBasisCode);
        }
    }
}
