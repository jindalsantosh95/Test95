using System;
using System.Linq;
using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability
{
    public static class SeatFulfilmentOrderDocUpdateMapper
    {
        public static SeatFulfilmentResponse UpdateOrderDocMapper(AddResponseType addResponse, SeatV8AddSeatQuery seatRequest)
        {
            return new SeatFulfilmentResponse()
            {
                OrderItems = addResponse.SeatSuccess.Select(ss =>
                {
                    var seatInfo = seatRequest.SelectedSeatInfo.First(x => x.SeatNumber == ss.OrderNumber.OrderItemNumber.SeatId.SeatNumber && x.FlightIdRef == ss.OrderNumber.OrderItemNumber.SeatId.FlightIdRef);
                    return new OrderItems
                    {
                        FlightIdRef = ss.OrderNumber.OrderItemNumber.SeatId.FlightIdRef,
                        SeatNumber = ss.OrderNumber.OrderItemNumber.SeatId.SeatNumber,
                        OrderItemNumber = ss.OrderNumber.OrderItemNumber.OrderItemNumber,
                        ElectronicMiscellaneousDocumentNumber = ss.OrderNumber.OrderItemNumber?.DocumentInfo?.ElectronicMiscellaneousDocumentNumber,
                        CustomerRefId = seatInfo.CustomerRefId,
                        Amount = seatInfo.Amount,
                        PaymentDetails = new List<PaymentDetails>() { new PaymentDetails() { PaymentReferenceId = seatRequest.SeatTransactionIds.FirstOrDefault(item => item.SegmentId == ss.OrderNumber.OrderItemNumber.SeatId.FlightIdRef)?.TransactionId } },
                        SeatAssignmentType = "PrimeSale"
                    };
                }),
                OrderNumber = addResponse.SeatSuccess.First().OrderNumber.OrderNumber,
                OrderId = $"VS932{seatRequest.BookingReferences.RLOC}{DateTime.UtcNow:yy}"
            };
        }

        public static SeatFulfilmentResponse UpdateOrderDocMapper(AddResponseType addResponse, OrderSeatV8AddSeatQuery seatRequest, string seatBookingTime)
        {
            return new SeatFulfilmentResponse()
            {
                OrderItems = addResponse.SeatSuccess.Select(ss =>
                {
                    var seatInfo = seatRequest.OrderAmendments.First(x => x.FlightItems.First().Segments.SegmentId == ss.OrderNumber.OrderItemNumber.SeatId.FlightIdRef && x.FlightItems.First().CurrentSeatAssignment == ss.OrderNumber.OrderItemNumber.SeatId.SeatNumber);
                    var monetaryAmount = seatInfo.CustomerPrices.PriceDetails.First().Amount.MonetaryAmount;

                    return new OrderItems
                    {
                        FlightIdRef = ss.OrderNumber.OrderItemNumber.SeatId.FlightIdRef,
                        SeatNumber = ss.OrderNumber.OrderItemNumber.SeatId.SeatNumber,
                        OrderItemNumber = ss.OrderNumber.OrderItemNumber.OrderItemNumber,
                        ElectronicMiscellaneousDocumentNumber = ss.OrderNumber.OrderItemNumber?.DocumentInfo?.ElectronicMiscellaneousDocumentNumber,
                        CustomerRefId = seatInfo.CustomerReference.First(),
                        Amount = new()
                        {
                            MonetaryAmount = monetaryAmount
                        },
                        PaymentDetails = new List<PaymentDetails>() { new PaymentDetails()
                        {
                            PaymentReferenceId = seatRequest.SeatTransactionId.First(item => item.SegmentId == ss.OrderNumber.OrderItemNumber.SeatId.FlightIdRef).TransactionId,
                            PaymentProcessingStatus = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL,
                            PaymentCommitmentDateTime = monetaryAmount == "0.00" ? seatBookingTime : null,
                            Amount = monetaryAmount == "0.00" ? new()
                            {
                                BaseCurrency = seatInfo.CustomerPrices.PriceDetails.First().Amount.BaseCurrency,
                                MonetaryAmount = monetaryAmount
                            } : null
                        }},
                        SeatAssignmentType = "PostSale"
                    };
                }),
                OrderNumber = addResponse.SeatSuccess.First().OrderNumber.OrderNumber,
                OrderId = $"VS932{seatRequest.Rloc}{DateTime.UtcNow:yy}"
            };
        }
    }
}
