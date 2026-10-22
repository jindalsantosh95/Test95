using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.Infrastructure.Mapper
{
    public class RetrieveSeatMapRequestMapperTests
    {
        [Fact]
        public void MapSeatAvailabilityMSRequestOrderDoc_ShouldMapFlightSegmentsClassOfServiceAndCustomerDetails()
        {
            var orderCreateRQ = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\PositiveFlow\\OrderCreateRQ");
            var orderDoc = TestDataProvider.GetTestData<OrderRS>("NDCOrder\\OrderCreate\\PositiveFlow\\OrderDoc");

            var result = RetrieveSeatMapRequestMapper.MapSeatAvailabilityMSRequestOrderDoc(orderDoc.OrderDocument, orderCreateRQ, "131FAA");

            Assert.NotNull(result);
            Assert.Equal(2 , result.FlightSegments.Count());
            Assert.Contains(result.FlightSegments, x => x.OriginAirportCode == "LHR" && x.DestinationAirportCode == "AMS" && x.MarketingFlightNumber == "7048");
            Assert.Contains(result.FlightSegments, x => x.OriginAirportCode == "AMS" && x.DestinationAirportCode == "JFK" && x.MarketingFlightNumber == "7130");
            Assert.True(result.FlightSegments.All(x => x.ClassOfService == "T"));
            Assert.Single(result.CustomerDetails);
        }
    }
}
