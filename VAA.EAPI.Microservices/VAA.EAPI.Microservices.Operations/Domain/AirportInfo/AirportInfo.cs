using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Operations.Domain.AirportInfo
{
    public class AirportInfo
    {
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public string Region { get; set; }
        public ICollection<string> Aliases { get; set; } = new List<string>();
        public string AwardTktTypCd { get; set; }
        public string CityType { get; set; }
        public string CodeShareOnlyInd { get; set; }
        public string EurpnUnionCityInd { get; set; }
        public string NonStdFreqOfOperationInd { get; set; }
        public string NonStdFreqOfOperationText { get; set; }
        public string TimeZone { get; set; }
        public string UseTimeForAvailability { get; set; }
    }
}
