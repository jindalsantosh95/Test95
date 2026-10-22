using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class Booking
    {
        public string? Status { get; set; }
        public string? Id { get; set; }
        public Pos? Pos { get; set; }
        public string? ReservationType { get; set; }
        public IEnumerable<Product>? Product { get; set; }
        public IEnumerable<Passenger>? Passengers { get; set; }

        public static explicit operator Booking(TravelReservation source)
        {
            var passengers = source.Passengers?
                .SelectMany(psngr =>
                {
                    var passenger = (Passenger)psngr;
                    passenger.IsLeadPassenger = false;

                    if (psngr.Infant != null)
                    {
                        passenger.AssociatedInfantId = psngr.Infant.PassengerResourceId;
                    }

                    var list = new List<Passenger> { passenger };

                    if (psngr.Infant != null)
                    {
                        var infant = (Passenger)psngr.Infant;
                        infant.IsLeadPassenger = false;
                        list.Add(infant);
                    }

                    return list;
                })
                .ToList();

            // Find the first ADULT passenger.
            // If no ADULT exists, fall back to the first passenger.
            var leadPassenger = passengers?
                .FirstOrDefault(x => string.Equals(x.PassengerType?.Category, "ADULT", StringComparison.OrdinalIgnoreCase)) ?? passengers?.FirstOrDefault();

            // Set only the selected passenger as the lead passenger
            if (leadPassenger != null)
            {
                leadPassenger.IsLeadPassenger = true;
            }

            return new Booking
            {
                Id = source.ReservationResourceId,

                Pos = source.Trips?.Any() == true ? (Pos)(source.Trips.First(), source.Passengers) : null,
                ReservationType = source.Reservation?.Type ?? string.Empty,
                Passengers = passengers,
                Product = source.Trips?.Any() == true ? [(Product)source] : new List<Product>()
            };
        }
    }
}