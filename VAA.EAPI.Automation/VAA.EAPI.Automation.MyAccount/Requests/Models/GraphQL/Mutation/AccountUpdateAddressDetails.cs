using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation
{
    public sealed class AccountUpdateAddressDetails
    {
        [JsonProperty("updateAddressDetail")]
        public required UpdateMemberAddressRequestInput UpdateAddressDetail { get; set; }
    }

    public sealed class UpdateMemberAddressRequestInput
    {
        [JsonProperty("accountAddressDetail")]
        public required AccountAddressDetail AccountAddressDetail { get; set; }

        [JsonProperty("memberType")]
        public string? MemberType { get; set; }
    }

    public sealed class AccountAddressDetail
    {
        [JsonProperty("addressLine1")]
        public string? AddressLine1 { get; set; }

        [JsonProperty("addressLine2")]
        public string? AddressLine2 { get; set; }

        [JsonProperty("addressLine3")]
        public string? AddressLine3 { get; set; }

        [JsonProperty("addressType")]
        public string? AddressType { get; set; }

        [JsonProperty("region")]
        public string? Region { get; set; }

        [JsonProperty("preferredFlag")]
        public string? PreferredFlag { get; set; }

        [JsonProperty("postalCode")]
        public string? PostalCode { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("companyName")]
        public string? CompanyName { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("addressVerified")]
        public bool? AddressVerified { get; set; }
    }
}
