using Microsoft.Azure.Cosmos.Table;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual
{
    public class SkyTeamInboundDataEntity : TableEntity
    {
        public string Envelope { get; set; } = string.Empty;
        public string DecryptedPayload { get; set; } = string.Empty;
        public string ParsedAccrualRequest { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public string Producer { get; set; } = string.Empty;
        public string NlaMessageCreatedAtUtc { get; set; } = string.Empty;
        public string ProcessingStatus { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public string? ErrorDescription { get; set; }
        public string? TransactionId { get; set; }
        public string? MileDetails { get; set; }

        [IgnoreProperty]
        public SkyTeamInboundDataMileDetails? FeedbackMileDetails
        {
            get => string.IsNullOrWhiteSpace(MileDetails) ? null : Json.Deserialize<SkyTeamInboundDataMileDetails>(MileDetails);
            set => MileDetails = value is null ? null : Json.Serialize(value, ignoreNullValues: false);
        }
    }
}
