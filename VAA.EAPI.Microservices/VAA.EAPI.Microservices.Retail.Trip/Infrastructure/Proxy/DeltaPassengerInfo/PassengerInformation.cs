
namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    // Main Data Models
    public class PassengerInformation
    {
        public string ReservationResourceId { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }

        public static explicit operator PassengerInformation((Domain.PassengerDetails.PassengerDetailsRequest source, string tripBookingId) input)
        {
            var (source, tripBookingId) = input;
            if (source is null) return null;

            return new PassengerInformation()
            {
                ReservationResourceId = input.tripBookingId,
                Passengers = source.Passengers?.Select(x => (Passenger)x)
            };

        }
    }
}