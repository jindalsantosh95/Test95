using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain.Trips
{
    public class Booking
    {
        public string PnrLocator { get; set; }
        public Pos Pos { get; set; }
        public IEnumerable<Person> Passenger { get; set; }
        public IEnumerable<Slice> Slices { get; set; }
        public IEnumerable<SeatAssignment> SeatDetails { get; set; }
    }
}