
using Newtonsoft.Json;

namespace VAA.EAPI.LoungeManagement.SkyTeam.Domain
{
    public class ErrorOrAccessDenial
    {
        [JsonProperty("BenefitEligibilityFromAirlineRES ")]
        public string BenefitEligibilityFromAirlineRES {  get; set; }

        [JsonProperty("ErrorOrAccessDenialCodeFromAirlineRES")]
        public string ErrorOrAccessDenialCodeFromAirlineRES { get; set; }

        [JsonProperty("ErrorOrAccessDenialMessageFromAirlineRES")]
        public string ErrorOrAccessDenialMessageFromAirlineRES { get; set; }

        [JsonProperty("ErrorInformation")]
        public string ErrorInformation {  get; set; }
    }
}
