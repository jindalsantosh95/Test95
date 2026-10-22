using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FarePointCalc
{
    public class FareCalculationRequest
    {
        public IEnumerable<InfantInArms> InfantInArms { get; set; }
        public IEnumerable<Passenger> Passengers { get; set;}
        public IEnumerable<DtiSegmentInfo> DtiSegmentInfos { get; set; }
    }
}
