using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Query
{
    public class AccountPointDotComDetails
    {
        [JsonProperty("onBehalf")]
        public string? OnBehalf { get; set; }
    }
}

