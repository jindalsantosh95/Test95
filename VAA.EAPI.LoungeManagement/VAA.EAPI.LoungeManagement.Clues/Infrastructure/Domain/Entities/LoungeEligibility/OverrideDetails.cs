using Newtonsoft.Json;

namespace VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility
{
    public class OverrideDetails
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("remarks")]
        public string Remarks { get; set; }

        [JsonProperty("airlineToBeBilled")]
        public string AirlineToBeBilled { get; set; }
    }
}
