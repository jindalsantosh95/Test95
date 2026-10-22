using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.Microservice.CustomerTransaction
{

    public class RetrieveVoucherResponse
    {
        [JsonProperty("voucherName")]
        public string? VoucherName { get; set; }

        [JsonProperty("voucherNumber")]
        public string? VoucherNumber { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("startDate")]
        public string? StartDate { get; set; }

        [JsonProperty("expiryDate")]
        public string? ExpiryDate { get; set; }

        [JsonProperty("dateGifted")]
        public string? DateGifted { get; set; }

        [JsonProperty("source")]
        public string? Source { get; set; }

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("maxValue")]
        public int? MaxValue { get; set; }

        [JsonProperty("usedDate")]
        public string? UsedDate { get; set; }

        [JsonProperty("isGifted")]
        public bool? IsGifted { get; set; }
    }


}
