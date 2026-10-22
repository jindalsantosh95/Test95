using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Infrastructure.PSS.Models
{
    public class Pnr
    {
        public List<HistoryCreditOrSignature> HistoryCreditOrSignatures { get; set; }
        public List<TicketInformation> TicketInformation { get; set; }
        public List<Passenger> Passengers { get; set; }
        public List<TicketData> Tickets { get; set; }
        public List<PassengerTypeInformation> Ptcs { get; set; }
        public List<InfantInArms> InfantInArms { get; set; }
        public IEnumerable<FareCalc> FareCalcs { get; set; }
        public IEnumerable<Dti> Dtis { get; set; }
        public IEnumerable<SegmentDetail> SegmentDetails { get; set; }
        public IEnumerable<FrequentFlyer> FrequentFlyer { get; set; }
        public IEnumerable<DtiSegmentInfo> DtiSegmentInfos { get; set; }
    }
}
