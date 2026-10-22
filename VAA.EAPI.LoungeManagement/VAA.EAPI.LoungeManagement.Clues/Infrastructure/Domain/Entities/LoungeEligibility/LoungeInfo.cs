using Newtonsoft.Json;

namespace VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility
{
    public class LoungeInfo
    {
        [JsonProperty("loungeId")]
        public string LoungeId { get; set; }

        [JsonProperty("operatorID")]
        public string OperatorId { get; set; }

        [JsonProperty("loungeOwner")]
        public string LoungeOwner { get; set; }

        [JsonProperty("locationCode")]
        public string LocationCode { get; set; }

        [JsonProperty("agentId")]
        public string AgentId { get; set; }
    }
}
