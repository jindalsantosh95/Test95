namespace VAA.EAPI.Web.Search.Models.OfferMS
{
    public class OriginDestination
    {
        public string OriginAirport { get; set; }
        public string OriginAirportName { get; set; }
        public string OriginCityName { get; set; }
        public string OriginCountryName { get; set; }
        public string DestinationAirport { get; set; }
        public string DestinationAirportName { get; set; }
        public string DestinationCityName { get; set; }
        public string DestinationCountryName { get; set; }
        public string DepartureDate { get; set; }
        public DepartureTimeRange DepartureTimeRange { get; set; }
    }
}
