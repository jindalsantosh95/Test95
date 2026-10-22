using VAA.EAPI.Microservices.Retail.Trip.Domain.Enum;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class Product
    {
        public string? BookingType { get; set; }
        public Reference? Reference { get; set; }
        public IEnumerable<Slice>? Slices { get; set; }
        public IEnumerable<Passenger>? Passengers { get; set; }
        public string? PropertyId { get; set; }
        public string? PropertyName { get; set; }
        public Address? Address { get; set; }
        public string? CheckIn { get; set; }
        public string? CheckOut { get; set; }
        public string? RoomType { get; set; }
        public string? RatePlan { get; set; }
        public TotalPrice? TotalPrice { get; set; }
        public string? Status { get; set; }
        public bool CheckInEligible { get; set; }

        public static explicit operator Product(TravelReservation source)
        {
            return new Product
            {
                BookingType = ProductTypes.AIRLINE.ToString(),
                Reference = (Reference)source.Reservation,
                Slices = source.Trips?.Select(trip => (Slice)trip),
                CheckInEligible = source.Reservation?.CheckinEligible ?? false
            };
        }
    }
}