using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests
{
    public class OrderDocumentMapperTests
    {
        [Fact]
        public void TestDisruptionMapper()
        {
            // Arrange
            var getReservRS = TestDataProvider.GetXmlTestData<GetReservationRS>("SabreOrderChangeMgt\\DisruptedGetReservRSTimeChange");
            var req = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt\\DisruptedOrderChangeReqTimeChange");

            // Act
            var orderDocument = OrderDocumentMapper.DisruptionOrderDocMapper(getReservRS , req);

            // Assert
            Assert.NotNull(orderDocument);
            Assert.Equal("HK", orderDocument.DataList.SegmentDetails.First(x => x.OriginAirportCode == "LHR" && x.DestinationAirportCode == "ATL").Status.Code);
            Assert.Equal("HK", orderDocument.DataList.SegmentDetails.First(x => x.OriginAirportCode == "ATL" && x.DestinationAirportCode == "LHR").Status.Code);
        }

        [Fact]
        public void DisruptionOrderDocMapper_WithNoTicketingInfo_ReturnsUnticketedOrder()
        {
            // Arrange
            var getReservRS = TestDataProvider.GetXmlTestData<GetReservationRS>("SabreOrderChangeMgt\\DisruptedGetReservRSTimeChange");
            var req = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt\\DisruptedOrderChangeReqTimeChange");
            ((ReservationPNRB)getReservRS.Item).PassengerReservation.TicketingInfo = null;

            // Act
            var orderDocument = OrderDocumentMapper.DisruptionOrderDocMapper(getReservRS, req);

            // Assert
            Assert.NotNull(orderDocument);
            Assert.Equal(string.Empty, orderDocument.OrderStatus);
        }

        [Fact]
        public void DisruptionOrderDocMapper_WithEmptyTicketDetails_ReturnsUnticketedOrder()
        {
            // Arrange
            var getReservRS = TestDataProvider.GetXmlTestData<GetReservationRS>("SabreOrderChangeMgt\\DisruptedGetReservRSTimeChange");
            var req = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt\\DisruptedOrderChangeReqTimeChange");
            ((ReservationPNRB)getReservRS.Item).PassengerReservation.TicketingInfo.TicketDetails = [];

            // Act
            var orderDocument = OrderDocumentMapper.DisruptionOrderDocMapper(getReservRS, req);

            // Assert
            Assert.NotNull(orderDocument);
            Assert.Equal(string.Empty, orderDocument.OrderStatus);
        }
    }
}
