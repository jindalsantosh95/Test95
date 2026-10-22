using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class TripEligibility
    {
        public int TripId { get; set; }
        public IEnumerable<CertificateEligibility> CertificatesEligibility { get; set; }
        public IEnumerable<string> IneligibleReasonCodes { get; set; }
        public IEnumerable<FlightSegmentEligibility> FlightSegmentsEligibility { get; set; }
    }
}
