using HotChocolate;
using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public sealed class RedemptionOption
    {
        public int OptionId { get; set; }
        public string? OptionName { get; set; }
        public string? RedemptionType { get; set; }
        public IEnumerable<AppliedVoucher>? AppliedVouchers { get; set; }
        public int? CoveredPassengers { get; set; }
        [GraphQLName("baseLinePoints")]
        [JsonProperty("baseLinePoints")]
        public long BaselinePoints { get; set; }
        public long PointsToPay { get; set; }
        public long TotalPointsUsed { get; set; }
        public decimal CashToPay { get; set; }
    }
}

