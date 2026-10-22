using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public partial class CustomerService
    {
        [JsonProperty("SegmentNo")]
        public string? SegmentNo { get; set; }

        [JsonProperty("BaggageFlag")]
        public string? BaggageFlag { get; set; }

        [JsonProperty("UpgradeFlag")]
        public string? UpgradeFlag { get; set; }

        [JsonProperty("TypeOfUpgrade")]
        public string? TypeOfUpgrade { get; set; }

        [JsonProperty("UpgradeFrom")]
        public string? UpgradeFrom { get; set; }

        [JsonProperty("UpgradeTo")]
        public string? UpgradeTo { get; set; }

        [JsonProperty("PreSelectMeal")]
        public string? PreSelectMeal { get; set; }

        [JsonProperty("PrefferedSeatBooked")]
        public string? PrefferedSeatBooked { get; set; }

        [JsonProperty("ClubHouseEligibilityDescription")]
        public string? ClubHouseEligibilityDescription { get; set; }

        [JsonProperty("ClubHouseEntryDateTime")]
        public string? ClubHouseEntryDateTime { get; set; }

        [JsonProperty("ClubHouseUsageIndicator")]
        public string? ClubHouseUsageIndicator { get; set; }

        [JsonProperty("ClubHouseLoungeName")]
        public string? ClubHouseLoungeName { get; set; }
    }
}
