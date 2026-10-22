using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.PSS.Entities.SeatAvailability
{
    public class SeatMap
    {
        public string ClassType { get; set; }
        public string Equipment { get; set; }
        public string Location { get; set; }
        public string SeatConfiguration { get; set; }
        public List<SeatRow> SeatRows { get; set; }
    }
}
