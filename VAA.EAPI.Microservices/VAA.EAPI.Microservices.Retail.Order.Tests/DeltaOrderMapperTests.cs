using System;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Mapper.FlightOrderCreate;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Tests
{
    public class DeltaOrderMapperTests
    {
        [Fact]
        public void OrderRequestMapper_ShouldSucceed_WithAllDetails()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderCreateRequest>("OrderMS\\OrderMsRequest");

            //Act
            var result = DeltaOrderMapper.OrderRequestMapper(request);

            Assert.NotNull(result);
            Assert.NotNull(result.PointOfSale);
            Assert.NotNull(result.SelectedOffers);
            Assert.NotNull(result.FormsOfPayment);
            Assert.NotNull(result.Customers);
        }
        [Fact]
        public void OrderResponseMapper_ShouldSucceed_WithAllDetails()
        {
            //Arrange
            var response = TestDataProvider.GetTestData<DlOrderResponse>("OrderMS\\DeltaOrderResponse");
            var request = TestDataProvider.GetTestData<OrderCreateRequest>("OrderMS\\OrderMsRequest");

            //Act
            var result = DeltaOrderMapper.OrderResponseMapper(response, request);

            Assert.NotNull(result);
            Assert.NotNull(result.PointOfSale);
            Assert.NotNull(result.OrderItems);
            Assert.NotNull(result.DataList);
            Assert.NotNull(result.BookingReference);
            Assert.True(result.PointOfSale.CurrencyCode == "GBP");
            Assert.True(result.OrderItems.Count() == 1);
            Assert.True(result.BookingReference.RLOC != null);
        }
        [Fact]
        public async Task OrderRequestMapper_ShouldThrowNullException_WithNullRequest()
        {
            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(async () => DeltaOrderMapper.OrderRequestMapper(null));
        }

        [Fact]
        public void OrderResponseMapper_ShouldSucceed_Validate_MultiCityOriginDestination()
        {

            //Arrange
            var response = TestDataProvider.GetTestData<DlOrderResponse>("OrderMS\\DeltaOrderMultiCityResponse");
            var request = TestDataProvider.GetTestData<OrderCreateRequest>("OrderMS\\OrderMsRequest");

            //Act
            var result = DeltaOrderMapper.OrderResponseMapper(response, request);

            Assert.NotNull(result);
            Assert.NotNull(result.PointOfSale);
            Assert.NotNull(result.DataList);

            // Validate multi-city trips
            Assert.NotNull(result.DataList.OriginDestinationAssociations);
            Assert.True(result.DataList.OriginDestinationAssociations.Count() > 2, "The number of trips should be more than two to validate multi-city.");

            // Validate OriginDestination
            var expectedOriginDestinations = response.Orders.First().OrderDataList.Trips.Select(t => new { t.OriginAirportCode, t.DestinationAirportCode }).ToList();
            var actualOriginDestinations = result.DataList.OriginDestinationAssociations.Select(od => new { od.OriginDestination.OriginAirport, od.OriginDestination.DestinationAirport }).ToList();

            Assert.Equal(expectedOriginDestinations.Count, actualOriginDestinations.Count);
            foreach (var expected in expectedOriginDestinations)
            {
                Assert.Contains(actualOriginDestinations, actual => actual.OriginAirport == expected.OriginAirportCode && actual.DestinationAirport == expected.DestinationAirportCode);
            }
        }

        [Fact]
        public void OrderResponseMapper_ShouldSucceed_Validate_MultiCitySegments()
        {

            //Arrange
            var response = TestDataProvider.GetTestData<DlOrderResponse>("OrderMS\\DeltaOrderMultiCityResponse");
            var request = TestDataProvider.GetTestData<OrderCreateRequest>("OrderMS\\OrderMsRequest");

            //Act
            var result = DeltaOrderMapper.OrderResponseMapper(response, request);

            Assert.NotNull(result);
            Assert.NotNull(result.PointOfSale);
            Assert.NotNull(result.DataList);

            // Validate Segments and Legs
            var expectedSegments = response.Orders.First().OrderDataList.PassengerItinerarySegment.SelectMany(segment => segment.FlightLeg.Select(leg => new
            {
                segment.FlightSegmentId,
                segment.OriginAirportCode,
                segment.DestinationAirportCode,
                leg.OriginAirport,
                leg.DestinationAirport,
                leg.OperatingCarrierCode,
                leg.LegId
            })).ToList();

            var actualSegments = result.DataList.SegmentDetails.SelectMany(segment => segment.Legs.Select(leg => new
            {
                segment.SegmentId,
                segment.OriginAirportCode,
                segment.DestinationAirportCode,
                LegOriginAirportCode = leg.OriginAirportCode,
                LegDestinationAirportCode = leg.DestinationAirportCode,
                ScheduledDepartureTime = leg.ScheduledDepartureTime.LocalDateTime,
                ScheduledArrivalTime = leg.ScheduledArrivalTime.LocalDateTime,
                leg.OperatingCarrier,
                leg.LegSequenceNumber
            })).ToList();

            Assert.Equal(expectedSegments.Count, actualSegments.Count);
            foreach (var expected in expectedSegments)
            {
                Assert.Contains(actualSegments, actual =>
                    actual.SegmentId == expected.FlightSegmentId &&
                    actual.OriginAirportCode == expected.OriginAirportCode &&
                    actual.DestinationAirportCode == expected.DestinationAirportCode &&
                    actual.OriginAirportCode == expected.OriginAirportCode &&
                    actual.DestinationAirportCode == expected.DestinationAirportCode &&
                    actual.OperatingCarrier == expected.OperatingCarrierCode &&
                    actual.LegSequenceNumber == expected.LegId
                );
            }
        }
    }
}
