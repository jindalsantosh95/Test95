using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR
{
    public class Passenger
    {
        public string PassengerResourceId { get; set; }
        public string GivenNames { get; set; }
        public string MiddleName { get; set; }
        public string Surname { get; set; }
        public string FirstNameNum { get; set; }
        public string LastNameNum { get; set; }
        public IEnumerable<SSRSegment> SSRSegments { get; set; }
    }
}
