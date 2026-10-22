using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class Location
    {
        public string Code { get; set; }
        public string Type { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public string ResortCode { get; set; }
        public string ResortName { get; set; }
        public string RegionCode { get; set; }
        public string RegionName { get; set; }
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string Address { get; set; }

        public static explicit operator Location(JToken source)
        {
            if (source == null) return null;

            return new Location()
            {
                Code = source["code"]?.Value<string>(),
                Type = source["type"]?.Value<string>(),
                CityCode = source["cityCode"]?.Value<string>(),
                CityName = source["cityName"]?.Value<string>(),
                CountryCode = source["countryCode"]?.Value<string>(),
                CountryName = source["countryName"]?.Value<string>(),
                StateCode = source["stateCode"]?.Value<string>(),
                StateName = source["stateName"]?.Value<string>(),
                ResortCode = source["resortCode"]?.Value<string>(),
                ResortName = source["resortName"]?.Value<string>(),
                RegionCode = source["regionCode"]?.Value<string>(),
                RegionName = source["regionName"]?.Value<string>(),
                AirportCode = source["airportCode"]?.Value<string>(),
                AirportName = source["airportName"]?.Value<string>(),
                Address = source["address"]?.Value<string>()
            };
        }
    }
}