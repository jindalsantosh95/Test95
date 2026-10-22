using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Query
{
    public class AccountValidatePassword
    {
        [JsonProperty("password")]
        public PasswordValidatorRequestInput? Password { get; set; }
    }

    public class PasswordValidatorRequestInput
    {
        [JsonProperty("password")]
        public string? Password { get; set; }
    }
}

