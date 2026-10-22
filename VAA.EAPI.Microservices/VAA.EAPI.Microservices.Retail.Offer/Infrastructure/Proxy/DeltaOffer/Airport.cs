using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Airport
    {
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public Distance AirportRadiusMileCnt { get; set; }
        public IEnumerable<AirportTerminal> AirportTerminals { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public string CountrySubdivisionCode { get; set; }
        public string IcaoAirportCode { get; set; }
        public decimal LatitudeDegreeNum { get; set; }
        public string LatitudeDirectionCode { get; set; }
        public decimal LongitudeDegreeNum { get; set; }
        public string LongitudeDirectionCode { get; set; }
        public string TimeZoneDesc { get; set; }
    }
}
