using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR;

namespace VAA.EAPI.Microservices.Flight.Services.Models.SSR
{
    public class PassengerSSRResponse
    {
        public IEnumerable<Passenger> Passengers { get; set; }

        public static explicit operator PassengerSSRResponse(DeltaSSRResponse source)
        {
            if (source?.SpecialServiceRequests?.Passengers == null)
                return new PassengerSSRResponse { Passengers = Enumerable.Empty<Passenger>() };

            return new PassengerSSRResponse
            {
                Passengers = source.SpecialServiceRequests?.Passengers.Select(p => (Passenger)p)
            };
        }
    }
}
