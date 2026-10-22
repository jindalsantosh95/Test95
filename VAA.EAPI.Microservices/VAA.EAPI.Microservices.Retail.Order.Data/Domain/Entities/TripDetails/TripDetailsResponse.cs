using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.TripDetails
{
    public class TripDetailsResponse
    {
        public string? Status { get; set; }
        public string? Id { get; set; }
        public string? ReservationType { get; set; }
        public Pos? Pos { get; set; }
        public IEnumerable<Product>? Product { get; set; }
        public IEnumerable<Passenger>? Passengers { get; set; }

        public static explicit operator TripDetailsResponse(OrderDoc orderDoc)
        {
            return new()
            {
                Pos = new() { CityCode = orderDoc?.OriginalPOS?.CityCode, CountryCode = orderDoc?.OriginalPOS?.CountryCode },
                Product = [new Product() { Reference = new() { AirlineDesignator = orderDoc.OrderDetails?.OwnerAirline, PnrLocator = orderDoc.OrderDetails?.BookingReferences?.FirstOrDefault()?.RLOC, CreatedDate = orderDoc.OrderDetails?.BookingReferences?.FirstOrDefault()?.RLOCCreateDate } }],
                Passengers = orderDoc.CustomerDetails.Select(p => (Passenger)p)
            };
        }
    }
}