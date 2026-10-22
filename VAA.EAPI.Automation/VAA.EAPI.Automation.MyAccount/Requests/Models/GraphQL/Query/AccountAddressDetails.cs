using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Query
{
    public class AccountAddressDetails
    {
        [JsonProperty("request")]
        public AddressDetailRequestInput? Request { get; set; }
    }

    public class AddressDetailRequestInput
    {
        [JsonProperty("addressLine1")]
        public string? AddressLine1 { get; set; }

        [JsonProperty("countryCode")]
        public string? CountryCode { get; set; }

        [JsonProperty("postCode")]
        public string? PostCode { get; set; }
    }
}

