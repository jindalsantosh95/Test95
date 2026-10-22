using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Query
{
    public class RetrieveHouseHoldAccount
    {
        [JsonProperty("retrieveHhAccountRequest")]
        public RetrieveHHAccountRequestInput? RetrieveHhAccountRequest { get; set; }
    }

    public class RetrieveHHAccountRequestInput
    {
        [JsonProperty("houseHoldFcNumber")]
        public string? HouseHoldFcNumber { get; set; }

        [JsonProperty("memberType")]
        public string? MemberType { get; set; }
    }
}

