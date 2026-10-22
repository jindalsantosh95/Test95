using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Query
{
    public sealed class ValidatePcmEligibility
    {
        [JsonProperty("uniqueId")]
        public string? UniqueId { get; set; }
    }
}
