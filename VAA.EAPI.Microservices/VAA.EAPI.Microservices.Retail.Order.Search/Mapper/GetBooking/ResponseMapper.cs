using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Response;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Mapper.GetBooking
{
    public class ResponseMapper
    {
        public static BookingResponse FilterLastNext(IEnumerable<OrderDoc> orders, TimeProvider timeProvider)
        {
            DateTime breakDate = DiagnosticsBreakDate(timeProvider);

            BookingResponse response = new();

            var earliestFutureOrder = orders?.SelectMany(f => f.SegmentDetails, (order, orderSegmentdetail) => new { order, orderSegmentdetail })
                                    .Where(y => DateTimeHelper.Air4DepartDateFormatConversion(y.orderSegmentdetail.DepartureDate, y.orderSegmentdetail.SegmentDepartureYear, y.orderSegmentdetail.DepartureTime) >= breakDate)
                                    .OrderBy(m => DateTimeHelper.Air4DepartDateFormatConversion(m.orderSegmentdetail.DepartureDate, m.orderSegmentdetail.SegmentDepartureYear, m.orderSegmentdetail.DepartureTime))
                                    .Select(n => new OrderDetail() { OrderID = n.order.OrderID, SegmentDetails = n.orderSegmentdetail, CustomerDetails = n.order.CustomerDetails, Order = n.order.OrderDetails, OriginalPos = n.order.OriginalPOS }).FirstOrDefault();

            var latestFlownOrder = orders?.SelectMany(f => f.SegmentDetails, (order, orderSegmentdetail) => new { order, orderSegmentdetail })
                                    .Where(a => DateTimeHelper.Air4DepartDateFormatConversion(a.orderSegmentdetail.DepartureDate, a.orderSegmentdetail.SegmentDepartureYear, a.orderSegmentdetail.DepartureTime) < breakDate)
                                    .OrderByDescending(b => DateTimeHelper.Air4DepartDateFormatConversion(b.orderSegmentdetail.DepartureDate, b.orderSegmentdetail.SegmentDepartureYear, b.orderSegmentdetail.DepartureTime))
                                    .Select(n => new OrderDetail() { OrderID = n.order.OrderID, SegmentDetails = n.orderSegmentdetail, CustomerDetails = n.order.CustomerDetails, Order = n.order.OrderDetails, OriginalPos = n.order.OriginalPOS }).FirstOrDefault();

            var items = new List<OrderDetail>()
            {
                earliestFutureOrder,
                latestFlownOrder
            };

            BookingResponse getBookingResponse = new()
            {
                VaaBookingDetails = BookingDetailMapper.MapBookingDetails(items.Where(x => x != null)),
                VaaCustomerDetails = CustomerDetailMapper.MapCustomerDetails(items.Where(x => x != null))
            };

            return getBookingResponse;
        }

        public static BookingResponse FilterFuture(IEnumerable<OrderDoc> orders, TimeProvider timeProvider)
        {
            DateTime breakDate = DiagnosticsBreakDate(timeProvider);


            BookingResponse response = new();

            var futureFlights = orders?.SelectMany(f => f.SegmentDetails, (order, orderSegmentdetail) => new { order, orderSegmentdetail })
                                 .Where(a => DateTimeHelper.Air4DepartDateFormatConversion(a.orderSegmentdetail.DepartureDate, a.orderSegmentdetail.SegmentDepartureYear, a.orderSegmentdetail.DepartureTime) >= breakDate)
                                 .OrderBy(b => DateTimeHelper.Air4DepartDateFormatConversion(b.orderSegmentdetail.DepartureDate, b.orderSegmentdetail.SegmentDepartureYear, b.orderSegmentdetail.DepartureTime))
                                 .Select(n => new OrderDetail() { OrderID = n.order.OrderID, SegmentDetails = n.orderSegmentdetail, CustomerDetails = n.order.CustomerDetails, Order = n.order.OrderDetails, OriginalPos = n.order.OriginalPOS })
                                .GroupBy(o => o.OrderID).Select(x => x.OrderBy(s => DateTimeHelper.Air4DepartDateFormatConversion(s.SegmentDetails.DepartureDate, s.SegmentDetails.SegmentDepartureYear, s.SegmentDetails.DepartureTime)).FirstOrDefault());

            return new()
            {
                VaaBookingDetails = BookingDetailMapper.MapBookingDetails(futureFlights.Where(x => x != null)),
                VaaCustomerDetails = CustomerDetailMapper.MapCustomerDetails(futureFlights.Where(x => x != null))
            };

        }

        private static DateTime DiagnosticsBreakDate(TimeProvider timeProvider)
        {
            var result = timeProvider.GetUtcNow().Date;

            try
            {
                if (AsyncContext.Current.Context.BindingContext.BindingData.TryGetValue("Headers", out var headersObj))
                {
                    var headers = Json.Deserialize<Dictionary<string, string>>((string)headersObj);
                    var normalizedKeyHeaders = headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => h.Value);
                    if (normalizedKeyHeaders.ContainsKey("api-refdate"))
                    {
                        if (DateTime.TryParseExact(normalizedKeyHeaders["api-refdate"], "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out DateTime breakDate))
                        {
                            result = breakDate;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing, this is predominantly for diagnostics use so if any of the gaurded code fails, it will
                // just default to UtcNow.Date
            }

            return result;
        }
    }
}

