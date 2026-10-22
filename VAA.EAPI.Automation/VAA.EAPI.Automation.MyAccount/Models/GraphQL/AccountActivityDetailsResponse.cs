using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Models.GraphQL
{
    public sealed class AccountActivityDetailsResponse
    {
        [JsonProperty("data")]
        public AccountActivityDetailsResponseData? Data { get; set; }
    }

    public sealed class AccountActivityDetailsResponseData
    {
        [JsonProperty("accountActivityDetails")]
        public ActivityDetail[]? AccountActivityDetails { get; set; }
    }
}

