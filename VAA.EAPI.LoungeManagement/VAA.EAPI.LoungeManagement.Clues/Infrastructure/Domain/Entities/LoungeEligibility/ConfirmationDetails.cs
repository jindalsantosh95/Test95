using Newtonsoft.Json;

namespace VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility
{
    public class ConfirmationDetails
    {
        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("entityAirlineCode")]
        public string EntityAirlineCode { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }
}
