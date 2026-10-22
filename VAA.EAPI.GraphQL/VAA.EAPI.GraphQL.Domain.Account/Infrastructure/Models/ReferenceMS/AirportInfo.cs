namespace VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS
{
    [RefDataType("airportdetail", "airportdetail", "airportCodes")]
    public class AirportInfo
    {
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string PosCityCode { get; set; }

    }
}
