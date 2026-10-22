using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.PSS.Entities.SeatAvailability
{
    public class SeatRow
    {
        public string Number { get; set; }
        public bool ExitOnLeft { get; set; }
        public bool ExitOnRight { get; set; }
        public bool Missing { get; set; }
        public bool OverWing { get; set; }
        public List<SeatColumn> SeatColumns { get; set; }
    }
}
