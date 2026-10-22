using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Query
{
    public class AccountMemberDetails
    {
        [JsonProperty("vouchersCount")]
        public int? VouchersCount { get; set; }
        [JsonProperty("activitiesCount")]
        public int? ActivitiesCount { get; set; }
        [JsonProperty("flightsCount")]
        public int? FlightsCount { get; set; }
    }
}
