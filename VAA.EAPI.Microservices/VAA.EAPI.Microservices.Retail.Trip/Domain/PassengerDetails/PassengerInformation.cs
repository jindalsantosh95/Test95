using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class PassengerDetailsRequest
    {
        public string bookingId { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }

        public static explicit operator PassengerDetailsRequest(DeltaPaxInfo.PassengerInformation source)
        {
            if (source is null) return null;

            return new PassengerDetailsRequest()
            {
                bookingId = source.ReservationResourceId,
                Passengers = source.Passengers?.Select(x => (Passenger)x)
            };
        }
    }
}