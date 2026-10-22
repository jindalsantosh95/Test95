using Moq;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Retail.Order.Search.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class OrderItemsMapperTests
    {
        private readonly Mock<IProductName> _mockproductName;
        public OrderItemsMapperTests()
        {
            _mockproductName = new Mock<IProductName>();
        }

        [Fact]
        public void Test_MultplePassengers()
        {
            //Arrange 
            var request = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");
            var ticketResponse = TestDataProvider.GetTestData<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType>("OrderSearchMS\\TicketingRetreiveResponse");
            bool isGDSbooking = request.TeardownContextResponse.Pnr.RecordLocator.First()?.OutsideSystemRecordLocator != null;
            List<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType> ticketResponseList = new();
            List<OrderSearchSegmentInfo> orderSearchSegmentInfos = new List<OrderSearchSegmentInfo>();
            orderSearchSegmentInfos.Add(new OrderSearchSegmentInfo() { DtiSegNo = "01", SegDlSegNo = "01", SegAddInfoSegNo = "01" });
            ticketResponseList.Add(ticketResponse);
            //Act
            var result = OrderItemsMapper.Air4ToOrderSearchResponseMapper(request, request.TeardownContextResponse.Pnr.Passenger, "001", new List<OrderInstructions>(), _mockproductName.Object, orderSearchSegmentInfos, ticketResponseList, isGDSbooking);

            //Assert
            Assert.Equal(5, result.Count);
            Assert.Equal("dummy0101", result.First(x => x.AccountableDocumentRefs.First().CustomerRefId == "dummy1").AccountableDocumentRefs.First().Coupons.First(x => x.SegmentRefId == "01").FareBasis);
            Assert.Equal("CH", result.First(x => x.AccountableDocumentRefs.First().CustomerRefId == "dummy3").AccountableDocumentRefs.First().Coupons.First(x => x.SegmentRefId == "01").Designator);
            Assert.Equal("dummy02", result.First(x => x.AccountableDocumentRefs.First().CustomerRefId == "dummy2").AccountableDocumentRefs.First().FareCalculation);
        }
        [Fact]
        public void Test_Conjuction()
        {
            //Arrange 
            var request = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponseForConjuction");
            bool isGDSbooking = request.TeardownContextResponse.Pnr.RecordLocator.First()?.OutsideSystemRecordLocator != null;
            var ticketResponse = TestDataProvider.GetTestData<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType>("OrderSearchMS\\TicketingRetreiveResponse");
            List<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType> ticketResponseList = new();
            ticketResponseList.Add(ticketResponse);
            List<OrderSearchSegmentInfo> orderSearchSegmentInfos = new List<OrderSearchSegmentInfo>();
            orderSearchSegmentInfos.Add(new OrderSearchSegmentInfo() { DtiSegNo = "01", SegDlSegNo = "01", SegAddInfoSegNo = "01" });
            orderSearchSegmentInfos.Add(new OrderSearchSegmentInfo() { DtiSegNo = "02", SegDlSegNo = "02", SegAddInfoSegNo = "02" });
            orderSearchSegmentInfos.Add(new OrderSearchSegmentInfo() { DtiSegNo = "03", SegDlSegNo = "03", SegAddInfoSegNo = "03" });
            orderSearchSegmentInfos.Add(new OrderSearchSegmentInfo() { DtiSegNo = "04", SegDlSegNo = "04", SegAddInfoSegNo = "04" });
            orderSearchSegmentInfos.Add(new OrderSearchSegmentInfo() { DtiSegNo = "05", SegDlSegNo = "05", SegAddInfoSegNo = "05" });
            //Act
            var result = OrderItemsMapper.Air4ToOrderSearchResponseMapper(request, request.TeardownContextResponse.Pnr.Passenger, "001", new List<OrderInstructions>(), _mockproductName.Object, orderSearchSegmentInfos, ticketResponseList, isGDSbooking);

            //Assert          
            Assert.Equal("dummy", result.First(x => x.AccountableDocumentRefs.First().CustomerRefId == "dummy1").AccountableDocumentRefs.First().Coupons.First(x => x.SegmentRefId == "01").FareBasis);
        }
    }
}
