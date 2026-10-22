using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Models.GraphQL
{
    public sealed class AccountAddressDetailsResponse
    {
        [JsonProperty("data")]
        public AccountAddressDetailsResponseData? Data { get; set; }
    }

    public sealed class AccountAddressDetailsResponseData
    {
        [JsonProperty("accountAddressDetails")]
        public Address[]? AccountAddressDetails { get; set; }
    }
}

