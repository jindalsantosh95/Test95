using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{

    public class Pos
    {
        public string CityCode { get; set; }
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
        public string DefaultCityCode { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public string BrowserInfo { get; set; }
        public string CurrencyNumber { get; set; }
        public string NDCCityCodeLocation { get; set; }

        public static explicit operator Pos(JObject source)
        {
            return new Pos()
            {
                CityCode = source["CityCode"]?.ToString(),
                CountryCode = source["CountryCode"]?.ToString(),
                CurrencyCode = source["CurrencyCode"]?.ToString(),
                DefaultCityCode = source["DefaultCityCode"]?.ToString(),
                DeviceInfo = source["DeviceInfo"]?.ToString(),
                IpAddress = source["IpAddress"]?.ToString(),
                BrowserInfo = source["BrowserInfo"]?.ToString(),
                CurrencyNumber = source["CurrencyNumber"]?.ToString(),
                NDCCityCodeLocation = source["NDCCityCodeLocation"]?.ToString()
            };
        }
    }

}