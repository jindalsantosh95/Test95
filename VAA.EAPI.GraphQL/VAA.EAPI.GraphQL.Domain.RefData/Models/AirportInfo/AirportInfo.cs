using HotChocolate;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Domain.RefData.Models.AirportInfo
{
    public sealed class AirportInfo
    {
        private const string WordRegexPattern = "(?<!\\w){0}\\w*";
        public static bool operator ==(AirportInfo obj1, string obj2) => obj1.Equals(obj2);
        public static bool operator !=(AirportInfo obj1, string obj2) => !obj1.Equals(obj2);

        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string AwardTktTypCd { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string CityServe { get; set; }
        public string CityType { get; set; }
        public string CodeShareOnlyInd { get; set; }
        public string PosCityCode { get; set; }
        public string CountryCode { get; set; }
        public string Region => CountryName;
        [GraphQLIgnore]
        public string CountryName { get; set; }
        public string EurpnUnionCityInd { get; set; }
        public string NonStdFreqOfOperationInd { get; set; }
        public string NonStdFreqOfOperationText { get; set; }
        public string TimeZone { get; set; }
        public string UseTimeForAvailability { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string[] Aliases
        {
            get => (CityServe?.Split('/', System.StringSplitOptions.RemoveEmptyEntries) ?? []).Concat([$"{CityName}, {Region} ({AirportCode})"]).ToArray();
            set { }
        }
        public string AirportFullName => $"{CityName}, {Region} ({AirportCode})";
        [JsonIgnore]
        public decimal Weight { get; set; }

        private VaaOperation vaaOperated = VaaOperation.None;

        [JsonIgnore]
        public VaaOperation VaaOperated { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is string)
            {
                var value = obj as string;
                var pattern = string.Join(@"\s+", value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => string.Format(WordRegexPattern, Regex.Escape(x))));

                return RegexCompiled.IsMatch(CityName.Replace("(", string.Empty).Replace(")", string.Empty), pattern, RegexOptions.IgnoreCase)
                    || (CityServe?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? []).Any(x => RegexCompiled.IsMatch(x.Replace("(", string.Empty).Replace(")", string.Empty), pattern, RegexOptions.IgnoreCase))
                    || RegexCompiled.IsMatch(AirportCode, pattern, RegexOptions.IgnoreCase)
                    || RegexCompiled.IsMatch(Region, pattern, RegexOptions.IgnoreCase);
            }

            return base.Equals(obj);
        }
    }

    [Flags]
    public enum VaaOperation : int
    {
        None = 0,
        City = 1,
        Airport = 2,
        Both = 3
    }
}