using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation
{
    public class UpdatePreference
    {
        [JsonProperty("updatePreferenceRequest")]
        public required UpdatePreferenceRequest UpdatePreferenceRequest { get; set; }
    }

    public class UpdatePreferenceRequest
    {
        [JsonProperty("preferences")]
        public required Preference[] Preferences { get; set; }

        [JsonProperty("updatedBy")]
        public required string UpdatedBy { get; set; }
    }

    public class Preference
    {
        [JsonProperty("preferenceType")]
        public required string PreferenceType { get; set; }

        [JsonProperty("preferenceValue")]
        public string? PreferenceValue { get; set; }
    }
}


