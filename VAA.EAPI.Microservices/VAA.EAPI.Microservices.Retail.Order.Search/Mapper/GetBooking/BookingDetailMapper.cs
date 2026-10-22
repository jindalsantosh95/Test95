using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Response;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Mapper.GetBooking
{
    public class BookingDetailMapper
    {
        public static List<VaaBookingDetail> MapBookingDetails(IEnumerable<OrderDetail> orderDetail)
        {
            List<VaaBookingDetail> bookingDetails = new();

            foreach (var order in orderDetail)
            {
                if (order.SegmentDetails != null)
                {
                    VaaBookingDetail vaaBookingDetail = new()
                    {
                        OrderId = order.OrderID,
                        FarePrice = new()
                        {
                            DtiSegmentInfo = MapDtiSegmentInfo(order),
                            FarePriceDetails = MapFarePriceDetail(order)
                        },
                        TotalFeesAndTaxes = new()
                        {
                            BaseCurrency = order.Order.TotalFeesAndTaxes.BaseCurrency
                        },
                        Rloc = order.OrderID[..6],
                        RlocCreateDate = order.OrderID.Substring(6, 7),
                        IsOldBooking = DateTimeHelper.Air4DepartDateFormatConversion(order.SegmentDetails.DepartureDate, order.SegmentDetails.SegmentDepartureYear, order.SegmentDetails.DepartureTime) < DateTime.Now ? "Y" : "N",
                        OriginalPos = new()
                        {
                            AgencyId = order.OriginalPos.AgencyId,
                            AgentId = order.OriginalPos.AgentId
                        },
                        VaaBookingCreationDateTime = new()
                        {
                            LocalDateTime = order.Order.CreationDateTime.LocalDateTime,
                            UtcDateTime = order.Order.CreationDateTime.UTCDateTime
                        },
                        VaaBookingSegmentDetails = MapSegmentDetails(order)
                    };
                    bookingDetails.Add(vaaBookingDetail);
                }
            }

            return bookingDetails;
        }

        public static List<VaaBookingSegmentDetail> MapSegmentDetails(OrderDetail orderDetail)
        {
            List<VaaBookingSegmentDetail> vaaBookingSegmentDetails = new();

            vaaBookingSegmentDetails.Add(new VaaBookingSegmentDetail()
            {
                FlightNumber = orderDetail.SegmentDetails.FlightNumber,
                ArrivalPoint = orderDetail.SegmentDetails.ArrivalPoint,
                ClassOfService = orderDetail.SegmentDetails.ClassOfService,
                DepartureDate = $"{orderDetail.SegmentDetails.DepartureDate}{orderDetail.SegmentDetails.SegmentDepartureYear}",
                DeparturePoint = orderDetail.SegmentDetails.DeparturePoint,
                DepartureTime = orderDetail.SegmentDetails.DepartureTime,
                MarketingCarrier = orderDetail.SegmentDetails.MarketingCarrier,
                OperatingCarrier = orderDetail.SegmentDetails.OperatingCarrier
            });

            return vaaBookingSegmentDetails;
        }

        public static List<DtiSegmentInfo> MapDtiSegmentInfo(OrderDetail orderDetail)
        {
            List<DtiSegmentInfo> dtiSegmentInfos = new();
            foreach (var segment in orderDetail.Order.FarePrice.DtiSegmentInfo)
            {
                dtiSegmentInfos.Add(new DtiSegmentInfo()
                {
                    BookNumber = segment.BookNumber,
                    Carrier = segment.Carrier,
                    FareBasisFareClassCode = segment.FarebasisFareclassCode
                });
            }

            return dtiSegmentInfos;
        }

        public static List<FarePriceDetail> MapFarePriceDetail(OrderDetail orderDetail)
        {
            List<FarePriceDetail> farePriceDetails = new();
            foreach (var farePrice in orderDetail.Order.FarePrice.FarePriceDetails)
            {
                farePriceDetails.Add(new FarePriceDetail()
                {
                    TotalFareCurrency = farePrice.TotalFareCurrency,
                    BaseCurrency = farePrice.BaseCurrency
                });
            }

            return farePriceDetails;
        }
    }
}
