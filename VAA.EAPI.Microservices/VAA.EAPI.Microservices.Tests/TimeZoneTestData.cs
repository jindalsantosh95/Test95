using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Microservices.Tests
{
    internal class TimeZoneTestData : TheoryData<string, string, bool>
    {
        private const string RegexPattern = @"  ";

        public TimeZoneTestData()
        {
            var airportCodes = Json.Deserialize<JObject>(Properties.Resources.vaa_routes).SelectTokens("$.data.airportList.items[*].airportCode")
                                                                                         .Select(x => x.Value<string>())
                                                                                         .ToArray();

            var timeZones = Json.Deserialize<JObject>(Properties.Resources.AirportDetails).SelectTokens("$.AirportDetails[*]")
                                                                                          .AsParallel()
                                                                                          .Select(x => (x["airportCode"]!.Value<string>(), RegexCompiled.Replace(x["timeZone"]!.Value<string>()!.Trim(), RegexPattern, " "), airportCodes.Contains(x["airportCode"]!.Value<string>())))
                                                                                          .Distinct();
            AddRange(timeZones);
        }
    }
}