using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class OriginDestinationInformation
    {
        public IEnumerable<FlightSegment> FlightSegment { get; set; }
    }
}
