using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation
{
    public sealed class AccountCreateYoungFlyerAccount
    {
        [JsonProperty("request")]
        public required CreateCustomerAccountRequestInput Request { get; set; }
    }

    public sealed class CreateCustomerAccountRequestInput
    {
        [JsonProperty("channelId")]
        public required string ChannelId { get; set; }

        [JsonProperty("channelUserId")]
        public required string ChannelUserId { get; set; }

        [JsonProperty("customerDetails")]
        public required YoungFlyerCustomerDetails CustomerDetails { get; set; }
    }

    public sealed class YoungFlyerCustomerDetails
    {
        [JsonProperty("firstName")]
        public required string FirstName { get; set; }

        [JsonProperty("lastName")]
        public required string LastName { get; set; }

        [JsonProperty("gender")]
        public required string Gender { get; set; }

        [JsonProperty("customerType")]
        public required string CustomerType { get; set; }

        [JsonProperty("dateOfBirth")]
        public required string DateOfBirth { get; set; }

        [JsonProperty("loyaltyMember")]
        public required YoungFlyerLoyaltyMember LoyaltyMember { get; set; }

        [JsonProperty("preferences")]
        public required object[] Preferences { get; set; }
    }

    public sealed class YoungFlyerLoyaltyMember
    {
        [JsonProperty("memberType")]
        public required string MemberType { get; set; }

        [JsonProperty("mediaCode")]
        public required string MediaCode { get; set; }

        [JsonProperty("enrollmentChannel")]
        public required string EnrollmentChannel { get; set; }

        [JsonProperty("termsAndConditionsAccepted")]
        public required bool TermsAndConditionsAccepted { get; set; }

        [JsonProperty("status")]
        public required string Status { get; set; }

        [JsonProperty("programName")]
        public required string ProgramName { get; set; }
    }
}
