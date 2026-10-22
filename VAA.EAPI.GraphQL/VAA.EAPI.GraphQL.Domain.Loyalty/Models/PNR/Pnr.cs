using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PNR
{
    public class Pnr
    {
        public IEnumerable<TicketInformation> TicketInformation { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public IEnumerable<TicketData> Tickets { get; set; }
        public IEnumerable<PassengerTypeInformation> Ptcs { get; set; }
        public IEnumerable<Dti> Dtis { get; set; }
        public IEnumerable<SegmentDetail> SegmentDetails { get; set; }
        public IEnumerable<InfantInArms> InfantInArms { get; set; }
        public IEnumerable<FrequentFlyer> FrequentFlyer { get; set; }
        public IEnumerable<DtiSegmentInfo> DtiSegmentInfos { get; set; }
    }
}
