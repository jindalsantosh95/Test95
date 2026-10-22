using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class TripsCertificateEligibilityInfo
    {
        public string EligibilityTrackingId { get; set; }
        public bool TripWithEligibilityIndicator { get; set; }
        public IEnumerable<TripEligibility> TripsEligibility { get; set; }

    }
}
