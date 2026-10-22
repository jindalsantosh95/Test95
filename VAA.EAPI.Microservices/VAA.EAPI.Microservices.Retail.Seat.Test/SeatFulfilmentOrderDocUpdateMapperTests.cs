using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Seat.Tests
{
    public class SeatFulfilmentOrderDocUpdateMapperTests
    {
        [Fact]
        public void UpdateOrderDocumentTest()
        {
            //Arrange
            var seatFulfilmentRequest = TestDataProvider.GetTestData<SeatV8AddSeatQuery>("SeatMS\\SeatFulfilmentRequest-Cash");
            var seatFulfilmentResponse = TestDataProvider.GetXmlTestData<AddResponseType>("SeatMS\\SeatV8AddResponse");

            //Act
            var result = SeatFulfilmentOrderDocUpdateMapper.UpdateOrderDocMapper(seatFulfilmentResponse , seatFulfilmentRequest);

            //Assert
            Assert.Equal(seatFulfilmentResponse.SeatSuccess.First().OrderNumber.OrderNumber , result.OrderNumber);
            Assert.Equal($"VS932{seatFulfilmentRequest.BookingReferences.RLOC}{DateTime.UtcNow:yy}", result.OrderId);

            foreach(var seat in seatFulfilmentResponse.SeatSuccess)
            {
                if(seatFulfilmentRequest.SelectedSeatInfo.Any(x => x.SeatNumber == seat.OrderNumber.OrderItemNumber.SeatId.SeatNumber))
                {
                    Assert.Contains(result.OrderItems, x => x.CustomerRefId == seatFulfilmentRequest.SelectedSeatInfo.First(x => x.SeatNumber == seat.OrderNumber.OrderItemNumber.SeatId.SeatNumber).CustomerRefId);

                    var orderItemInResult = result.OrderItems.First(x => x.CustomerRefId == seatFulfilmentRequest.SelectedSeatInfo.First(x => x.SeatNumber == seat.OrderNumber.OrderItemNumber.SeatId.SeatNumber).CustomerRefId);

                    Assert.Equal(seat.OrderNumber.OrderItemNumber.OrderItemNumber, orderItemInResult.OrderItemNumber);
                    Assert.Equal(seat.OrderNumber.OrderItemNumber.SeatId.SeatNumber, orderItemInResult.SeatNumber);
                    Assert.Equal(seat.OrderNumber.OrderItemNumber.SeatId.FlightIdRef, orderItemInResult.FlightIdRef);
                    Assert.Equal(seat.OrderNumber.OrderItemNumber.DocumentInfo.ElectronicMiscellaneousDocumentNumber, orderItemInResult.ElectronicMiscellaneousDocumentNumber);
                    Assert.Equal(seatFulfilmentRequest.SelectedSeatInfo.First(x => x.SeatNumber == seat.OrderNumber.OrderItemNumber.SeatId.SeatNumber).Amount, orderItemInResult.Amount);

                }
            }
        }
    }
}
