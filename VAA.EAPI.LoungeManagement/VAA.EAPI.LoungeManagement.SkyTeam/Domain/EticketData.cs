using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.LoungeManagement.SkyTeam.Domain
{
    public class EticketData
    {
        [JsonProperty("ETKTNumber")]
        public string ETKTNumber { get; set; }

        [JsonProperty("PassengerFirstName ")]
        public string PassengerFirstName { get; set; }

        [JsonProperty("PassengerLastName ")]
        public string PassengerLastName { get; set; }

        [JsonProperty("FlightSegmentDetails")]
        public IEnumerable<FlightSegmentDetails> FlightSegmentDetails { get; set; }

        [JsonProperty("ErrorOrAccessDenialCode")]
        public string ErrorOrAccessDenialCode { get; set; }

        [JsonProperty("ErrorOrAccessDenialMessage")]
        public string ErrorOrAccessDenialMessage { get; set; }

    }
}
