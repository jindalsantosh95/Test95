using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual
{
    public sealed class AutoAccrualFeedbackPayload
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public string ProcessedStatus { get; set; } = string.Empty;
        public AutoAccrualFeedbackErrorDetails? ErrorDetails { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public AutoAccrualFeedbackMileDetails MileDetails { get; set; } = new();
    }

    public sealed class AutoAccrualFeedbackErrorDetails
    {
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorDescription { get; set; } = string.Empty;
    }

    public sealed class AutoAccrualFeedbackMileDetails
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int BaseMiles { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int ClassOfServiceBonusMiles { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int EliteTierBonusMiles { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int PromotionalBonusMiles { get; set; }
    }
}
