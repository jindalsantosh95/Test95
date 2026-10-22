using Newtonsoft.Json;

namespace VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility
{
    public class Cabin
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("fareCode")]
        public string FareCode { get; set; }
    }
}
