using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation
{
    public sealed class UpdateMemberPhoneNumber
    {
        [JsonProperty("updatePhoneNumber")]
        public required UpdatePhoneNumberInput UpdatePhoneNumber { get; set; }
    }

    public sealed class UpdatePhoneNumberInput
    {
        [JsonProperty("countryCode")]
        public required string CountryCode { get; set; }

        [JsonProperty("phoneNumber")]
        public required string PhoneNumber { get; set; }
    }
}
