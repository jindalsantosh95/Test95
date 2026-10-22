using System.Collections.Generic;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS;


namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA
{

    public class SeatMap : Cabin
    {
        public IEnumerable<Cabin> CabinList { get; set; }

        public IEnumerable<SeatRow> SeatRows { get; set; }

    }
}
