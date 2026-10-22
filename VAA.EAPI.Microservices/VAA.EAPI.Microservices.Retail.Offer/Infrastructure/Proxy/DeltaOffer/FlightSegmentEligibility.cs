using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class FlightSegmentEligibility
    {
        public int FlightSegmentId { get; set; }
        public string UpgradeBrandId { get; set; }
        public bool DominantSegment { get; set; }
        public IEnumerable<SegmentCertificateEligibility> CertificatesEligibility { get; set; }
        public IEnumerable<string> IneligibleReasonCodes { get; set; }
    }
}
