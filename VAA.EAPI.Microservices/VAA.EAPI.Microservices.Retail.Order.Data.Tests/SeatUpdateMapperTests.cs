using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCSeatUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper.NDCSeatUpdate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class SeatUpdateMapperTests
    {
        [Fact]
        public void UpdateOrderDocumentTest()
        {
            //Arrange
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDoc");
            var seatResponse = TestDataProvider.GetTestData<SeatFulfilmentResponse>("SeatMS\\SeatFulfilmentResponse-Cash");

            //Act
            var updatedOrderDoc = SeatUpdateMapper.UpdateOrderDocument(orderDoc, seatResponse);

            //Assert
            foreach(var item in seatResponse.OrderItems)
            {
                Assert.Contains(updatedOrderDoc.OrderItems, x => x.AccountableDocumentRefs?.FirstOrDefault().DocumentNumber == item.ElectronicMiscellaneousDocumentNumber);

                var orderItemInResult = updatedOrderDoc.OrderItems.First(x => x.AccountableDocumentRefs?.FirstOrDefault().DocumentNumber == item.ElectronicMiscellaneousDocumentNumber);

                Assert.Equal(item.Amount.MonetaryAmount.Insert(item.Amount.MonetaryAmount.Length - item.Amount.DecimalPrecisionCount,".") , orderItemInResult.PriceDetails.First().Amount.MonetaryAmount);
                Assert.Equal(item.SeatNumber , orderItemInResult.Services.First().CurrentSeatAssignment);
                Assert.Equal($"SEG{item.FlightIdRef.TrimStart('0')}", orderItemInResult.Services.First().ProductSegmentAssociation.flightSegmentRefIds.First());
                Assert.Equal(EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum, orderItemInResult.Services.First().Product.ProductType);
                Assert.Equal(item.CustomerRefId , orderItemInResult.AccountableDocumentRefs.First().CustomerRefId);
                Assert.Equal($"SEG{item.FlightIdRef.TrimStart('0')}" , orderItemInResult.AccountableDocumentRefs.First().Coupons.First().SegmentRefId);
                Assert.Equal(EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef.DocumentTypes.YEnum, orderItemInResult.AccountableDocumentRefs.First().DocumentType);
            }
        }
    }
}
