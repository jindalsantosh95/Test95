using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class Seat
    {
        public int? Row { get; set; }
        public string Column { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public IEnumerable<CabinClass> ActualCabinClass { get; set; }

    }
}
