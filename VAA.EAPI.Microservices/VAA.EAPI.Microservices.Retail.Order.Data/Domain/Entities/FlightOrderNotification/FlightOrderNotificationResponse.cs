using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightOrderNotification
{
    public class FlightOrderNotificationResponse
    {
        public Flight EventFlight { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public IEnumerable<Seat> Seats { get; set; }
        public string OrderId { get; set; }
        public bool Irop { get; set; }

        public static explicit operator FlightOrderNotificationResponse(OrderDoc orderDoc)
        {
            var passenger = orderDoc?.CustomerDetails?.FirstOrDefault(cd => cd.CustomerContacts?.Contacts?
                            .Any(c => !string.IsNullOrWhiteSpace(c.Email)) == true) ?? orderDoc?.CustomerDetails?.FirstOrDefault();

            return new FlightOrderNotificationResponse
            {
                BookingReference = (BookingReference)orderDoc,
                Passengers = passenger == null ? null : new List<Passenger>
                                {
                                new()
                                {
                                    FirstName = passenger?.FirstName,
                                    LastName = passenger?.LastName,
                                    Email = passenger?.CustomerContacts?.Contacts?.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Email))?.Email
                                }                
                                },
                OrderId = orderDoc?.OrderID,
                Irop = orderDoc?.OrderHistorydetails?.Remarks?.Any(r => r.FreeformText?.Contains("IROP") ?? false) ?? false
            };
        }

    }
}
