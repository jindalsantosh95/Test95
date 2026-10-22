using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Flight.Services.Models.SSR
{
    public class SSRSegment
    {
        public FlightSegment FlightSegment { get; set; }
        public IEnumerable<ServiceRequest> ServiceRequests { get; set; }

        public static explicit operator SSRSegment(Infrastructure.Delta.SSR.SSRSegment sourceSegment)
        {
            if (sourceSegment == null)
                return null;

            return new SSRSegment
            {
                FlightSegment = (FlightSegment)sourceSegment.DatedOperatingSegment,
                ServiceRequests = sourceSegment.SpecialServiceRequests?.Select(sr => (ServiceRequest)sr)
            };
        }
    }
}
