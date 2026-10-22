using System;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightOrderNotification
{
    public class BookingReference
    {
        public string AirlineDesignator { get; set; }
        public string PnrLocator { get; set; }
        public DateTime CreatedDate { get; set; }
        public string BookingChannel { get; set; }

        public static explicit operator BookingReference(OrderDoc orderDoc)
        {
            return new BookingReference
            {
                AirlineDesignator = orderDoc?.OrderDetails?.OwnerAirline ?? "VS",
                PnrLocator = orderDoc?.OrderDetails?.BookingReferences?.FirstOrDefault()?.RLOC ?? "",
                CreatedDate = Convert.ToDateTime(orderDoc?.OrderDetails?.BookingReferences?.FirstOrDefault()?.RLOCCreateDate),
                BookingChannel = orderDoc?.OrderDetails?.BookingReferences?.FirstOrDefault()?.BookingChannel,
            };
        }
    }
}