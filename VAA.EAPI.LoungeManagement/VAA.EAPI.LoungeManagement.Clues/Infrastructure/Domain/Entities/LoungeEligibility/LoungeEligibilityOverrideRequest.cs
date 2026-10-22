using Newtonsoft.Json;

namespace VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility
{
    public class LoungeEligibilityOverrideRequest
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("trackId")]
        public string TrackId { get; set; }

        [JsonProperty("overrideDetails")]
        public OverrideDetails OverrideDetails { get; set; }
    }
}
