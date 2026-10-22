namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class BrandByFlightLeg
    {
        public string brandId { get; set; }
        public string cabin { get; set; }
        public string cabinPadisCode { get; set; }
        public string cosGroupCode { get; set; }
        public string cosCode { get; set; }
        public bool deltaProduct { get; set; }
        public bool hostAirlineProduct { get; set; }
        public string flightLegNum { get; set; }
        public string flightSegmentNum { get; set; }
        public List<Product> product { get; set; }
        public int priorityNum { get; set; }
        public string tripId { get; set; }
        public bool dominantLeg { get; set; }
        public List<object> globalUpgradeCertificateLegStatus { get; set; }
        public List<object> regionalUpgradeCertificateLegStatus { get; set; }
        public string flightSegmentId { get; set; }
    }
}
