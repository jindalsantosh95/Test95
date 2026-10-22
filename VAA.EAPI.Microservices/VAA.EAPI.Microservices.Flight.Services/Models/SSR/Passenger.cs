using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Flight.Services.Models.SSR
{
    public class Passenger
    {
        public string PassengerResourceId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<SSRSegment> SSRSegments { get; set; }

        public static explicit operator Passenger(Infrastructure.Delta.SSR.Passenger sourcePassenger)
        {
            if (sourcePassenger == null)
                return null;

            return new Passenger
            {
                PassengerResourceId = sourcePassenger.PassengerResourceId,
                FirstName = sourcePassenger.GivenNames,
                LastName = sourcePassenger.Surname,
                SSRSegments = sourcePassenger.SSRSegments?
                    .Select(s => (SSRSegment)s) ?? Enumerable.Empty<SSRSegment>()
            };
        }
    }
}
