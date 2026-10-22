using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCSeatUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper.NDCSeatUpdate
{
    public static class SeatUpdateMapper
    {
        public static OrderDocument UpdateOrderDocument(OrderDocument orderDoc, SeatFulfilmentResponse seatFulfilmentResponse)
        {
            if (seatFulfilmentResponse.Deletion)
            { 
                foreach(var seatOI in seatFulfilmentResponse.OrderItems)
                {
                    var orderItem = orderDoc.OrderItems.First(x => x.Services.First().Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && x.Services.First().CurrentSeatAssignment == seatOI.SeatNumber && x.Services.First().SegmentRefIds?.FirstOrDefault()?.Replace("SEG","")?.PadLeft(2,'0') == seatOI.FlightIdRef);
                    orderDoc.OrderItems = orderDoc.OrderItems.Where(oi => oi.OrderItemId != orderItem.OrderItemId);
                }
            }
            else
            {
                foreach (var seatOrderItem in seatFulfilmentResponse.OrderItems)
                {
                    orderDoc.OrderItems = orderDoc.OrderItems.Append(new EAPI.Domain.Virgin.Models.Order.OrderItems()
                    {
                        OrderItemId = $"{orderDoc.OrderItems.Last().OrderItemId.Split('-').First()}-{Convert.ToInt32(orderDoc.OrderItems.Last().OrderItemId.Split('-').Last()) + 1}",
                        OwnerAirline = Constants.Application.OwnerAirline,
                        AccountableDocumentRefs = [ new()
                    {
                        DocumentType = AccountableDocumentRef.DocumentTypes.YEnum,
                        DocumentNumber = seatOrderItem.ElectronicMiscellaneousDocumentNumber,
                        CustomerRefId = seatOrderItem.CustomerRefId,
                        Coupons = [ new() { CouponStatus = Constants.Application.OpenStatus , SegmentRefId = $"SEG{seatOrderItem.FlightIdRef.TrimStart('0')}" }],
                        IssueDate = DateTime.UtcNow.ToString(Constants.Application.DateFormat)
                    }],
                        PriceDetails = [ new()
                    {
                        Amount = new()
                        {
                            BaseCurrency = seatOrderItem.Amount.BaseCurrency,
                            MonetaryAmount = seatOrderItem.Amount.MonetaryAmount.Contains('.') || seatOrderItem.Amount.DecimalPrecisionCount == 0 ? seatOrderItem.Amount.MonetaryAmount : seatOrderItem.Amount.MonetaryAmount.Insert(seatOrderItem.Amount.MonetaryAmount.Length - seatOrderItem.Amount.DecimalPrecisionCount , ".")
                        },
                        PriceComponentType = EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum
                    }],
                        Services = [ new() {
                        ServiceId = "SV1",
                        CurrentSeatAssignment =  seatOrderItem.SeatNumber,
                        CurrentSeatAssignmentType = seatOrderItem.SeatAssignmentType,
                        ServiceBeneficiaries = [seatOrderItem.CustomerRefId],
                        ProductSegmentAssociation = new()
                        {
                            flightSegmentRefIds = [$"SEG{seatOrderItem.FlightIdRef.TrimStart('0')}"],
                        },
                        Product = new()
                        {
                           ProductType = EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum,
                           ProductOwner = Constants.Application.OwnerAirline
                        },
                        SegmentRefIds = [$"SEG{seatOrderItem.FlightIdRef.TrimStart('0')}"]
                    }],
                        PaymentDetails = seatOrderItem.PaymentDetails,
                    });
                }

                orderDoc.Version = orderDoc.Version + 1;
                orderDoc.ChangeType = ChangeTypeEnum.OrderItemAdded;
                orderDoc.ChangeDateTime = new() { UtcDateTime = DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) };
            }

            return orderDoc;
        }
    }
}
