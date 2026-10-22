
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings
{
    public class OrderDataList
    {
        public string OrderID { get; set; }
        public IEnumerable<SegmentDetail> Segments { get; set; }
        public IEnumerable<BookingReference> Bookingsref { get; set; }
    }
}
