using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual
{
    public sealed class AutoAccrualFeedbackOutboundMessage : TableEntity
    {
        public string Producer { get; set; } = string.Empty;

        public string CorrelationId { get; set; } = string.Empty;

        public string EncryptedPayload { get; set; } = string.Empty;

        public string? ParsedFeedbackRequest { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? ErrorCode { get; set; }
    }
}
