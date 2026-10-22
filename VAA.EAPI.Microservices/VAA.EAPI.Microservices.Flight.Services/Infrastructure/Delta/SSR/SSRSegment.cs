using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR
{
    public class SSRSegment
    {
        public DatedOperatingSegment DatedOperatingSegment { get; set; }
        public IEnumerable<SpecialServiceRequest> SpecialServiceRequests { get; set; }

        public static explicit operator SSRSegment(Models.SSR.SSRSegment sourceSegment)
        {
            if (sourceSegment == null)
                return null;

            return new SSRSegment
            {
                DatedOperatingSegment = (DatedOperatingSegment)sourceSegment.FlightSegment,
                SpecialServiceRequests = sourceSegment.ServiceRequests?.Select(sr => (SpecialServiceRequest)sr)
            };
        }
    }
}
