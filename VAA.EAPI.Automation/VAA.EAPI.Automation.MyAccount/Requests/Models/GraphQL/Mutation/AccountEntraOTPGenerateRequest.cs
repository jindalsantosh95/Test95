using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation
{
    public class AccountEntraOTPGenerateRequest
    {
        [JsonProperty("emailId")]
        public string EmailId { get; set; } = string.Empty;
    }
}
