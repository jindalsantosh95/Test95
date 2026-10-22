using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Models.GraphQL
{
    public class Category
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("sortOrder")]
        public int? SortOrder { get; set; }
    }

    public class DataObject
    {
        [JsonProperty("memberVoucherDetails")]
        public MemberVoucherDetails? MemberVoucherDetails { get; set; }
    }

    public class MemberVoucherDetails
    {
        [JsonProperty("categories")]
        public List<Category>? Categories { get; set; }

        [JsonProperty("vouchers")]
        public List<Voucher>? Vouchers { get; set; }

        [JsonProperty("memberTotalVoucherCount")]
        public int? MemberTotalVoucherCount { get; set; }

        [JsonProperty("pointsThreshold")]
        public int? PointsThreshold { get; set; }
    }

    public class MemberVoucherDetailsResponse
    {
        [JsonProperty("data")]
        public DataObject? Data { get; set; }
    }

    public class Voucher
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("issueDate")]
        public DateTime IssueDate { get; set; }

        [JsonProperty("expiryDate")]
        public DateTime ExpiryDate { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("isGifted")]
        public bool IsGifted { get; set; }

        [JsonProperty("points")]
        public string Points { get; set; }
    }


}
