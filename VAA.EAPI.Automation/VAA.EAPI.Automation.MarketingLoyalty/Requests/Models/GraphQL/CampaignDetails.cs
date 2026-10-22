using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL
{
    public class CampaignDetails
    {
        [JsonProperty("CampaignId")]
        public string CampaignId { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("StartDate")]
        public string StartDate { get; set; }

        [JsonProperty("EndDate")]
        public string EndDate { get; set; }

        [JsonProperty("CampaignCode")]
        public string CampaignCode { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("RegistrationRequired")]
        public string RegistrationRequired { get; set; }

        [JsonProperty("LinkToTemplate")]
        public string LinkToTemplate { get; set; }

        [JsonProperty("Promotion")]
        public Promotion Promotion { get; set; }


    }
}
