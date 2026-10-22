using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderSearchMS;

namespace VAA.EAPI.NDC.Order.Mapper.Order
{
    public class OrderItemStatusMapper
    {
        public static string GetStatusCode(string flightSegmentRefIds, IEnumerable<OrderItems> orderItems, bool isOrderChange , IEnumerable<Segment> segments)
        {
            var segment = segments.Where(seg => seg.SegmentId == flightSegmentRefIds.TrimStart('F'));

            string statusCode = null;

            if (segment.First().Status != null)
            {
                var ticketInfo = orderItems.First().AccountableDocumentRefs?.FirstOrDefault()?.Coupons?.Where(doc => doc?.SegmentRefId == segment.First().SegmentId);

                if (ticketInfo != null)
                {
                    if (Constants.Application.FlightStatusCodeRemoved == segment.First().Status.Code && (ticketInfo.First().CouponStatus == Constants.Application.CouponStatusVoid || ticketInfo.First().CouponStatus == Constants.Application.CouponStatusRefunded))
                    {
                        statusCode = Constants.Application.ServiceStatusCodeCancelled;
                    }
                }
                else if (Constants.Application.DeliveryStatusCodeRemoved == segment.First().Status.Code)
                {
                    statusCode = Constants.Application.ServiceStatusCodeCancelled;
                }
                if (Constants.Application.FlightServiceWLStatusCode.Contains(segment.First().Status.Code))
                {
                    statusCode = Constants.Application.ServiceStatusCodeWaitlisted;
                }
                else if (Constants.Application.FlightStatusCode.Contains(segment.First().Status.Code))
                {
                    statusCode = Constants.Application.DeliveryStatusCodeConfirmed;
                }
                else if (Constants.Application.FlightRequestedStatusCode.Contains(segment.First().Status.Code))
                {
                    statusCode = Constants.Application.DeliveryStatusCodeRequested;
                }
                else if(Constants.Application.FlightStatusCodeTransferred == segment.First().Status.Code)
                {
                    statusCode = Constants.Application.ServiceStatusCodeTransferred;
                }
                else if (Constants.Application.FlightStatusCodeRemoved == segment.First().Status.Code)
                {
                    statusCode = Constants.Application.ServiceStatusCodeCancelled;
                }
                else if (Constants.Application.FlightStatusCodeCancelled == segment.First().Status.Code)
                {
                    statusCode = Constants.Application.ServiceStatusCodeCancelled;
                }
                else
                {
                    statusCode = orderItems.First().Services.First().Status?.Status;
                }
            }

            return statusCode;
        }

        public static string GetDeliveryStatusCode(IEnumerable<OrderItems> orderItems, OrderService service, IEnumerable<Segment> segments)
        {
            var segment = segments.FirstOrDefault(x => service.SegmentRefIds.Contains(x.SegmentId));

            if (orderItems.Any(x => x.AccountableDocumentRefs != null))
            {
                foreach (var orderItem in orderItems.Where(x => x.AccountableDocumentRefs != null && (service.Product?.ProductType == null || (x.Services?.Any(srv => srv.Product?.ProductType == service.Product.ProductType) ?? false))))
                {
                    foreach (var accDoc in orderItem.AccountableDocumentRefs)
                    {
                        if (accDoc.Coupons == null) continue;

                        foreach (var coupon in accDoc.Coupons)
                        {
                            var seg = segments.FirstOrDefault(s => s.SegmentId == coupon.SegmentRefId);
                            if (seg == null) continue;

                            if (!service.ServiceBeneficiaries.Contains(accDoc.CustomerRefId) ||
                                !service.SegmentRefIds.Contains(seg.SegmentId))
                                continue;

                            var documentType = accDoc.DocumentType?.ToString()?.Replace(Constants.Application.TicketTypeEnumChar, string.Empty);

                            var ticketExpirationDate = coupon.TicketExpirationDate is null ? DateTime.Now : DateTime.ParseExact(coupon.TicketExpirationDate, "ddMMMyy", CultureInfo.InvariantCulture);

                            if (Constants.Application.FlightRequestedStatusCode.Contains(seg.Status?.Code))
                                return Constants.Application.DeliveryStatusCodeRequested;

                            if (seg.Status?.Code == Constants.Application.FlightStatusCodeRemoved)
                                return Constants.Application.DeliveryStatusCodeRemoved;

                            if (seg.Status?.Code == Constants.Application.FlightStatusCodeCancelled)
                                return Constants.Application.DeliveryStatusCodeRemoved;

                            if ((Constants.Application.FlightStatusCode.Contains(seg.Status?.Code) || seg.Status?.Code == Constants.Application.FlightStatusCodeRemoved || seg.Status?.Code == Constants.Application.FlightStatusCodeTransferred) && (coupon.CouponStatus == Constants.Application.CouponStatusOpen || coupon.CouponStatus == Constants.Application.SearchEMDOpenCouponStatus) && (documentType == Constants.Application.TicketTypeT || documentType == Constants.Application.SeatDocType))
                            {
                                return Constants.Application.DeliveryStatusCodeReadyToProceed;
                            }

                            if (Constants.Application.FlightStatusCode.Contains(seg.Status?.Code) && Constants.Application.CouponStatusReadyToDeliver.Contains(coupon.CouponStatus) && (documentType == Constants.Application.TicketTypeT || documentType == Constants.Application.SeatDocType))
                            {
                                return Constants.Application.DeliveryStatusCodeReadyToDeliver;
                            }

                            if (Constants.Application.FlightStatusCode.Contains(seg.Status?.Code) && coupon.CouponStatus == Constants.Application.CouponStatusFlwn && documentType == Constants.Application.TicketTypeT)
                            {
                                return Constants.Application.DeliveryStatusCodeDelivered;
                            }

                            if (coupon.CouponStatus == Constants.Application.FlownEMDCouponStatus && seg.Status.DeliveryStatusCode == "Delivered")
                            {
                                return Constants.Application.DeliveryStatusCodeDelivered;
                            }

                            if (coupon.CouponStatus == Constants.Application.CouponStatusVoid || coupon.CouponStatus == Constants.Application.CouponStatusRefunded || DateTime.Now > ticketExpirationDate)
                            {
                                return Constants.Application.DeliveryStatusCodeRemoved;
                            }
                        }
                    }
                }

                if(segment != null && Constants.Application.FlightStatusCode.Contains(segment.Status?.Code) && segment.Status.DeliveryStatusCode == "Delivered")
                    return Constants.Application.DeliveryStatusCodeDelivered;

                if (segment != null && Constants.Application.FlightStatusCode.Contains(segment.Status?.Code))
                    return Constants.Application.DeliveryStatusCodeConfirmed;
            }
            else if (segment != null)
            {
                if (Constants.Application.FlightRequestedStatusCode.Contains(segment.Status?.Code))
                    return Constants.Application.DeliveryStatusCodeRequested;

                if (Constants.Application.FlightStatusCode.Contains(segment.Status?.Code))
                    return Constants.Application.DeliveryStatusCodeConfirmed;

                if (segment.Status?.Code == Constants.Application.FlightStatusCodeCancelled)
                    return Constants.Application.DeliveryStatusCodeRemoved;

                if (segment.Status?.Code == Constants.Application.FlightStatusCodeRemoved)
                    return Constants.Application.DeliveryStatusCodeRemoved;

            }

            return null;
        }

    }
}
