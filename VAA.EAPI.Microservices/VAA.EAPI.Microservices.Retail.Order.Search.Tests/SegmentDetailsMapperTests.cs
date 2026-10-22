using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Retail.Order.Search.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class SegmentDetailsMapperTests
    {
        private readonly Mock<ICachedAirport> _mockCachedAirport;
        private readonly Mock<ICachedSegmentStatusCode> _mockCachedSegmentStatusCode;
        private readonly Mock<ICachedCabinType> _mockCachedCabinType;

        public SegmentDetailsMapperTests()
        {
            _mockCachedAirport = new Mock<ICachedAirport>();
            _mockCachedSegmentStatusCode = new Mock<ICachedSegmentStatusCode>();
            _mockCachedCabinType = new Mock<ICachedCabinType>();
            Arrange();
            OrderSearchTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task Test_SingleSegment()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\SingleSegment");
            List<Passenger> passengers = new();
            List<OriginDestinationAssociations> originDestinationAssociations = new();

            Dictionary<string, string> segmentODMapper = new();
            List<OrderSearchSegmentInfo> orderSearchSegmentInfos = new();
            orderSearchSegmentInfos.Add(new OrderSearchSegmentInfo() { DtiSegNo = "dummy", SegDlSegNo = "dummy", SegAddInfoSegNo = "dummy" });

            //Act
            var result = await SegmentDetailsMapper.Air4ToOrderSearchResponseMapper(request, passengers, _mockCachedAirport.Object, originDestinationAssociations, segmentODMapper, _mockCachedSegmentStatusCode.Object, _mockCachedCabinType.Object, orderSearchSegmentInfos);
            //Assert
            Assert.Single(result);
            Assert.Equal(string.Empty, result.First(x => x.MarketingFlightNumber == "dummy").AirCraft);
            Assert.Equal("JFK", result.First(x => x.MarketingFlightNumber == "dummy").DestinationAirportCode);
            Assert.Equal("John F Kennedy Intl", result.First(x => x.MarketingFlightNumber == "dummy").DestinationAirportName);
        }

        [Fact]
        public async Task Test_MultiSegments()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\MultiSegment");
            List<Passenger> passengers = new();
            List<OriginDestinationAssociations> originDestinationAssociations = new();
            Dictionary<string, string> segmentODMapper = new();
            List<OrderSearchSegmentInfo> orderSearchSegmentInfos = new();
            orderSearchSegmentInfos.Add(new OrderSearchSegmentInfo() { DtiSegNo = "dummy1", SegDlSegNo = "dummy1", SegAddInfoSegNo = "dummy1" });
            orderSearchSegmentInfos.Add(new OrderSearchSegmentInfo() { DtiSegNo = "02", SegDlSegNo = "02", SegAddInfoSegNo = "02" });

            //Act
            var result = await SegmentDetailsMapper.Air4ToOrderSearchResponseMapper(request, passengers, _mockCachedAirport.Object, originDestinationAssociations, segmentODMapper, _mockCachedSegmentStatusCode.Object, _mockCachedCabinType.Object, orderSearchSegmentInfos);
            //Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("02", result.First(x => x.MarketingFlightNumber == "dummy2").SegmentNumber);
            Assert.Equal(string.Empty, result.First(x => x.MarketingFlightNumber == "dummy2").AirCraft);
            Assert.Equal("JFK", result.First(x => x.MarketingFlightNumber == "dummy2").DestinationAirportCode);
            Assert.Equal("John F Kennedy Intl", result.First(x => x.MarketingFlightNumber == "dummy2").DestinationAirportName);
        }
        private void Arrange()
        {
            _mockCachedAirport.Setup(x => x.GetCachedAirport("LHR")).ReturnsAsync(new AirportLocation()
            {
                AirportCode = "LHR",
                AirportName = "Heathrow",
                CityCode = "LON",
                CityName = "LON",
                CountryCode = "GB",
                CountryName = "United Kingdom"
            });
            _mockCachedAirport.Setup(x => x.GetCachedAirport("LAS")).ReturnsAsync(new AirportLocation()
            {
                AirportCode = "LAS",
                AirportName = "Mccarran Intl",
                CityCode = "LAS",
                CityName = "LAS",
                CountryCode = "US",
                CountryName = "United States"
            });
            _mockCachedAirport.Setup(x => x.GetCachedAirport("JFK")).ReturnsAsync(new AirportLocation()
            {
                AirportCode = "JFK",
                AirportName = "John F Kennedy Intl",
                CityCode = "NYC",
                CityName = "NYC",
                CountryCode = "US",
                CountryName = "United States"
            });
        }
    }
}
