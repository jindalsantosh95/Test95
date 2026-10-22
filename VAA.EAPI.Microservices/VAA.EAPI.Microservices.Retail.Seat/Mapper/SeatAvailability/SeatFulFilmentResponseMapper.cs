using System.Linq;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;

namespace VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability
{
    public static class SeatFulFilmentResponseMapper
    {
        public static SeatFulfilmentResponse MapAddSeatResponse(IEnumerable<AddResponseType> addResponses, SeatV8AddSeatQuery seatRequest = null, OrderSeatV8AddSeatQuery orderSeatRequest = null, string seatBookingTime = null)
        {
            var response = new SeatFulfilmentResponse();
            List<OrderItems> orderItemList = new List<OrderItems>();
            List<Error> errors = new List<Error>();
            
            foreach (var addResponse in addResponses)
            {
                var orderItemNumberList = addResponse?.SeatSuccess?.Select(ss =>
                {
                    var seatInfo = seatRequest?.SelectedSeatInfo.FirstOrDefault(x => x.SeatNumber.PadLeft(3, '0') == ss.OrderNumber.OrderItemNumber.SeatId.SeatNumber.PadLeft(3, '0') && x.FlightIdRef.PadLeft(3,'0') == ss.OrderNumber.OrderItemNumber.SeatId.FlightIdRef.PadLeft(3,'0')); 

                    return new OrderItems
                    {
                        FlightIdRef = ss?.OrderNumber?.OrderItemNumber?.SeatId?.FlightIdRef,
                        SeatNumber = ss.OrderNumber?.OrderItemNumber?.SeatId?.SeatNumber,
                        OrderItemNumber = ss.OrderNumber?.OrderItemNumber?.OrderItemNumber,
                        ElectronicMiscellaneousDocumentNumber = ss?.OrderNumber?.OrderItemNumber?.DocumentInfo?.ElectronicMiscellaneousDocumentNumber,
                        CustomerRefId = seatInfo?.CustomerRefId,
                        Amount = seatInfo?.Amount,
                        PaymentDetails = new List<PaymentDetails>() { new PaymentDetails() { PaymentReferenceId = seatRequest != null ? seatRequest?.SeatTransactionIds?.FirstOrDefault(item => item.SegmentId == ss?.OrderNumber?.OrderItemNumber?.SeatId?.FlightIdRef)?.TransactionId : orderSeatRequest?.SeatTransactionId?.FirstOrDefault(item => item.SegmentId == ss?.OrderNumber?.OrderItemNumber?.SeatId?.FlightIdRef)?.TransactionId,
                    PaymentCommitmentDateTime = seatBookingTime
                    } }
                    };
                }).ToList();

                if (orderItemNumberList.IsNullOrDefault() == false)
                    orderItemList.AddRange(orderItemNumberList);
                else if(addResponse?.SeatError.IsNullOrDefault() == true)
                    errors.AddRange([
                        new Error()
                        {
                            IsWarning = "true",
                            Message = "INCOMPLETESEATADDRESPONSE"
                        }]);

                if (addResponse?.SeatError.IsNullOrDefault() == false)
                    errors.AddRange(addResponse?.SeatError?.Select(x =>
                    new Error()
                    {
                        IsWarning = "true",
                        Message = x.Error?.ErrorText
                    }));
            }
            
            response.OrderNumber = addResponses.FirstOrDefault(r => r.SeatSuccess != null)?.SeatSuccess?.FirstOrDefault()?.OrderNumber?.OrderNumber ?? addResponses.FirstOrDefault()?.ResponseInfo.FirstOrDefault()?.TransactionId;
            response.OrderItems = orderItemList;
            response.Error = errors;

            return response;
        }
    }
}
