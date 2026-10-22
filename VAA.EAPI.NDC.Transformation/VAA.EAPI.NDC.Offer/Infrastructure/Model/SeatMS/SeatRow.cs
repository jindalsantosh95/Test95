using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS
{
    public class SeatRow
    {
        public string RowNumber { get; set; }
        public bool ExitOnLeft { get; set; }
        public bool ExitOnRight { get; set; }
        public bool Missing { get; set; }
        public bool OverWing { get; set; }
        public bool UpperDeck { get; set; }
        public IEnumerable<SeatColumn> SeatColumns { get; set; }
    }
}
