using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS
{
    public class SsrSegment
    {
        public FlightSegment FlightSegment { get; set; }
        public IEnumerable<ServiceRequest> ServiceRequests { get; set; }

        public static explicit operator SsrSegment((SsrSegment ssrSegment, string ssrCode) source)
        {
            var (passengerSegment, ssrCode) = source;

            return new()
            {
                FlightSegment = new() { SegmentId = passengerSegment.FlightSegment.SegmentId },
                ServiceRequests = [new() { ServiceTypeCode = ssrCode, ServiceStatus = "HK" }]
            };
        }
    }
}