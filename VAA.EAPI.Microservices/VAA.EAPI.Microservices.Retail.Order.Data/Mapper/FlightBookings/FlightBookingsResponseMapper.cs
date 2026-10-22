using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper.FlightBookings
{
    public class FlightBookingsResponseMapper
    {
        public static FlightBookingsResponse FilterFuture(IEnumerable<OrderDoc> orders)
        {
            DateTime breakDate = DateTime.UtcNow.Date;

            if (orders.Any(x => x.SegmentDetails.Any()))
            {
                var futureFlights = orders?.SelectMany(f => f.SegmentDetails, (order, orderSegmentdetail) => new { order, orderSegmentdetail })
                                     .Where(a => DateTimeHelper.Air4DepartDateFormatConversion(a.orderSegmentdetail?.DepartureDate, a.orderSegmentdetail?.SegmentDepartureYear, a.orderSegmentdetail?.DepartureTime) >= breakDate)
                                     .OrderBy(b => DateTimeHelper.Air4DepartDateFormatConversion(b.orderSegmentdetail?.DepartureDate, b.orderSegmentdetail?.SegmentDepartureYear, b.orderSegmentdetail?.DepartureTime))
                                     .Select(n => new OrderDataList() { OrderID = n.order?.OrderID, Segments = n.order?.SegmentDetails, Bookingsref = n.order?.OrderDetails?.BookingReferences })
                                     .GroupBy(o => o.OrderID).Select(x => x.OrderBy(s => DateTimeHelper.Air4DepartDateFormatConversion(s.Segments?.FirstOrDefault()?.DepartureDate, s.Segments?.FirstOrDefault()?.SegmentDepartureYear, s.Segments?.FirstOrDefault()?.DepartureTime)).FirstOrDefault()).ToList();

                var futureflightdetails = MapBookingDetails(futureFlights.Where(x => x != null));

                if (futureflightdetails.Any())
                {

                    var response = new FlightBookingsResponse()
                    {
                        FlightBookingsDetails = futureflightdetails
                    };

                    response.FlightBookingsDetails = [.. (response.FlightBookingsDetails ?? Enumerable.Empty<FlightBookingsDetails>()).Select(booking =>
                                                      {
                                                        var order = orders?.FirstOrDefault(o => o.OrderID == booking.OrderID);
                                                        booking.CustomerDetails = order?.CustomerDetails ?? new List<CustomerDetail>();
                                                       return booking;
                                                        })];
                    return response;
                }

                else
                {
                    return new()
                    {
                        Message = Constants.Application.MsgDataNotFound
                    };


                }
            }
            else
            {
                return new()
                {
                    Message = Constants.Application.MsgDataNotFound
                };

            }


        }

        public static IEnumerable<FlightBookingsDetails> MapBookingDetails(IEnumerable<OrderDataList> orderDetail)
        {
            List<FlightBookingsDetails> futureList = new();

            foreach (OrderDataList order in orderDetail)
            {
                DateTime breakDate = DateTime.UtcNow.Date;

                var futureBooking = order.Segments.Where(x => DateTimeHelper.Air4DepartDateFormatConversion(x?.DepartureDate, x?.SegmentDepartureYear, x?.DepartureTime) >= breakDate);

                foreach (var x in futureBooking)
                {
                    FlightBookingsDetails item = new()
                    {
                        OrderID = order?.OrderID,
                        BookingReference = order?.Bookingsref.FirstOrDefault().RLOC,
                        OperatingCarrier = x?.OperatingCarrier,
                        OperatingFlightNumber = x?.OperatingFlightNumber,
                        OriginAirportCode = x.OriginAirportCode,
                        DestinationAirportCode = x.DestinationAirportCode,
                        BookingClass = x?.ClassOfService,
                        ScheduledDepartureTime = DateTimeHelper.Air4DepartDateFormatConversion(x?.DepartureDate, x?.SegmentDepartureYear, x?.DepartureTime).ToString(),
                        MarketingCarrier = x?.MarketingCarrier,
                        MarketingFlightNumber = x?.MarketingFlightNumber,
                        SegmentNumber = x?.SegmentNumber,     
                        BookingDate= order?.Bookingsref.FirstOrDefault().RLOCCreateDate
                    };
                    futureList.Add(item);
                }

            }
            return futureList;
        }

        public static IEnumerable<FlightBookingsDetails> MockCustomerBookingResponse()
        {
            return new List<FlightBookingsDetails>() {

               new FlightBookingsDetails{

                  OrderID = "DU74S719APR25",
                  BookingReference = "CWOB43",
                  OperatingCarrier = "VS",
                  OperatingFlightNumber = "153",
                  OriginAirportCode = "LHR",
                  DestinationAirportCode = "JFK",
                  BookingClass = "R",
                  ScheduledDepartureTime = "2025-04-19T13:50:00Z",
               },
               new FlightBookingsDetails{

                  OrderID = "DU74S728APR25",
                  BookingReference = "CWOB44",
                  OperatingCarrier = "VS",
                  OperatingFlightNumber = "154",
                  OriginAirportCode = "JFK",
                  DestinationAirportCode = "LHR",
                  BookingClass = "R",
                  ScheduledDepartureTime = "2025-04-28T13:50:00Z",
               }

            };

        }


    }
}
