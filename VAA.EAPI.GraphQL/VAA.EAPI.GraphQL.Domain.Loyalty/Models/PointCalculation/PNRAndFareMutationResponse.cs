using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PointCalculation
{
    public class PNRAndFareMutationResponse
    {
        public string PNR { get; set; }
        public int? TotalPoints { get; set; }
        public IEnumerable<PassengerFare> PaxFares { get; set; }
        public int NoOfSegments { get; set; }
        public IEnumerable<SegmentDtl> Segments { get; set; }
        public int NoOfPassengers { get; set; }

    }
}
