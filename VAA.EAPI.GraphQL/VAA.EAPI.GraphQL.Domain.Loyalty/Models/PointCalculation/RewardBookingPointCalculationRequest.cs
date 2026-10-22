using System.Collections.Generic;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PNR;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PointCalculation
{
    public class RewardBookingPointCalculationRequest
    {
        public IEnumerable<Passenger> Passengers { get; set; }
        public IEnumerable<InfantInArms> InfantInArms { get; set; }
        public IEnumerable<DtiSegmentInfo> DtiSegmentInfos { get; set; }
    }
}
