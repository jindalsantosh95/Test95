namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.CommonRefDB
{
    public class AirportDetails
    {
        public string IataAirportCode { get; set; }
        public string IcaoAirportCode { get; set; }
        public string Name { get; set; }
        public string CityIataCode { get; set; }
        public string RouteSwareAptNme { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public double? DecimalLatitude { get; set; }
        public double? DecimalLongitude { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public string GlobalRegion { get; set; }
        public int? Wac { get; set; }
        public string WacName { get; set; }
        public int? AirportAltitudeFt { get; set; }
        public string FromUtcOffset { get; set; }
        public string ToUtcOffset { get; set; }
        public string DstOffset { get; set; }
        public string ToUtcDtsOffset { get; set; }
        public string WikiLink { get; set; }
        public bool? IsDlHubYn { get; set; }
        public bool? IsAfklHubYn { get; set; }
        public bool? IsVsHubYn { get; set; }
    }
}
