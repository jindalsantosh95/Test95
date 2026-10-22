using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation
{
    public sealed class AccountUpdateEmailAddress
    {
        [JsonProperty("newEmailAddress")]
        public required UpdateEmailRequestInput NewEmailAddress { get; set; }
    }

    public sealed class UpdateEmailRequestInput
    {
        [JsonProperty("emailAddress")]
        public required string EmailAddress { get; set; }
    }
}
