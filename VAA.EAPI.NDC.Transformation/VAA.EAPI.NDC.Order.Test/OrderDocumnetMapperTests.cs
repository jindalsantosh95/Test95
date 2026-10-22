using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Order.Mapper.Order;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests
{
    public class OrderDocumentMapperTests
    {
        [Fact]
        public void OrderDocumnetMapperReturnTests()
        {
            //Arrange
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\NewOrderDoc");
            var newOrderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\OrderDoc_Change");

            //Act
            var request = OrderDocumentMapper.UpdateChangeOrderDocument(orderDoc, newOrderDoc);

            //Assert
            Assert.NotNull(request);
            Assert.Equal("SEG1NEW", request.DataList.SegmentDetails.First().SegmentId);
            Assert.Equal("SEG1NEW", request.OrderItems.Last().AccountableDocumentRefs.Last().Coupons.First().SegmentRefId);
            Assert.Equal("HX", request.DataList.SegmentDetails.Last().Status.Code);
            Assert.Equal("LHR",request.DataList.SegmentDetails.First().OriginAirportCode);
            Assert.Equal("ATL", request.DataList.SegmentDetails.First().DestinationAirportCode);
            Assert.NotNull(request.OrderItems.First().PaymentDetails);
            Assert.Equal("80878891", request.OrderItems.First().PaymentDetails.First().PaymentReferenceId);
        }

        [Fact]
        public void UpdateSeatProductDetails_Test()
        {
            //Arrange
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderCreate\\SeatOrderDoc"); 

            var seatRs = new SeatRS()
            {
                SeatAvailabilityResponse = new()
                {
                    FlightSegmentSeatMap = [new()
                    {
                        FlightSegment = new(){
                            SegmentId = "01"
                        },
                        Catalogue = [new(){
                            CatalogueId = "abc123",
                            ProductName = "Exit Seat",
                        }],
                        Seatmap = [new()
                        {
                            SeatRows = [new()
                            {
                                RowNumber = "46",
                                SeatColumns = [new()
                                {
                                    CatalogueRefIds = [ "abc123" ],
                                    Seat = true,
                                    SeatIdentifer = "46A",
                                    Attributes = [ "PREFERRED_SEAT","EXIT_SEAT" ]
                                }]
                            }]
                        }]
                    }]
                }
            };

            //Act
            var response = OrderDocumentMapper.UpdateSeatProductDetails(orderDoc, seatRs);
            
            //Assert
            Assert.NotNull(response);

            var seatOrderItem = response.OrderItems.First(a => a.Services.First().Product.ProductType.ToString() == "SeatEnum");
            Assert.Equal("Exit Seat", seatOrderItem.Services.First().Product.ProductName);
        }

        [Fact]
        public void UpdateSeatPaymentDetails_Test()
        {
            //Arrange
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderCreate\\SeatOrderDoc");

            var paymentRS = new PaymentRS()
            {
                PaymentDocument = new()
                {
                    TransactionId = "81681953",
                    PaymentCommitedDateTime = DateTime.UtcNow.ToString("yyyy-mm-dd"),
                    CardType = "VI"
                }
            };

            //Act
            var response = OrderDocumentMapper.UpdateSeatPaymentDetails(orderDoc, paymentRS);

            //Assert
            Assert.NotNull(response);

            var seatOrderItem = response.OrderItems.First(a => a.Services.First().Product.ProductType.ToString() == "SeatEnum");
            var seatPaymentDetails = seatOrderItem.PaymentDetails.First();
            
            Assert.NotNull(seatPaymentDetails.PaymentCommitmentDateTime);
            Assert.Equal("CardEnum", seatPaymentDetails.FormOfPayments.ToString());
            Assert.NotNull(seatPaymentDetails.CardMethod);
            Assert.Equal("VI", seatPaymentDetails.CardMethod.CardBrandCode);
        }

        [Fact]
        public void UpdateOTPaymentFunctions_WhenPaymentDetailsAreNull_AddsPaymentFunction()
        {
            var orderDocument = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\NewOrderDoc");
            foreach (var orderItem in orderDocument.OrderItems)
            {
                orderItem.PaymentDetails = null;
            }

            var response = OrderDocumentMapper.UpdateOTPaymentFunctions(orderDocument, orderDocument, false, false);

            Assert.NotNull(response.OrderItems.First().PaymentDetails);
            Assert.Single(response.OrderItems.First().PaymentDetails);
        }

        [Fact]
        public void UpdateOrderItemId_WhenHoldBookingItemEndsInA_IncrementsToB()
        {
            var oldOrderDocument = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\NewOrderDoc");
            var updatedOrderDocument = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\NewOrderDoc");
            var oldOrderItem = oldOrderDocument.OrderItems.First();
            oldOrderItem.OrderItemId = $"{oldOrderDocument.OrderNumber.OrderId}-1A";

            OrderDocumentMapper.UpdateOrderItemId(updatedOrderDocument, oldOrderDocument, isDisrupted: true);

            Assert.Equal(oldOrderItem.OrderItemId, updatedOrderDocument.OrderItems.First().ExistingOrderItemId);
            Assert.Equal($"{oldOrderDocument.OrderNumber.OrderId}-1B", updatedOrderDocument.OrderItems.First().OrderItemId);
        }
    }
}
