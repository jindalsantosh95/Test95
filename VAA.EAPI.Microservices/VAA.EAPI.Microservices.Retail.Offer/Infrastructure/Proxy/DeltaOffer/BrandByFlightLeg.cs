using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class BrandByFlightLeg
    {
        public string BrandId { get; set; }
        public string Cabin { get; set; }
        public string CabinPadisCode { get; set; }
        public string CosGroupCode { get; set; }
        public string CosCode { get; set; }
        public bool DeltaProduct { get; set; }
        public bool HostAirlineProduct { get; set; }
        public string FlightLegNum { get; set; }
        public string FlightSegmentNum { get; set; }
        public IEnumerable<Product> Product { get; set; }
        public int PriorityNum { get; set; }
        public string TripId { get; set; }
        public IEnumerable<UpgradeInformation> UpgradeOption { get; set; }
        public bool? DominantLeg { get; set; }
        public string ProductLongDesc { get; set; }
        public string ProductShortDesc { get; set; }
        public string ProductName { get; set; }
        public IEnumerable<UpgradeCertificateStatus> GlobalUpgradeCertificateLegStatus { get; set; }
        public IEnumerable<UpgradeCertificateStatus> RegionalUpgradeCertificateLegStatus { get; set; }
        public string FlightSegmentId { get; set; }
    }
}
