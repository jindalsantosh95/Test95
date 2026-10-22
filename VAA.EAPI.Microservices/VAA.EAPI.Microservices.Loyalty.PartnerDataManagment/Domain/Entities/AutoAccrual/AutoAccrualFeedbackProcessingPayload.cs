using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual
{
    public class AutoAccrualFeedbackProcessingPayload
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public string ProcessedStatus { get; set; } = string.Empty;
        public AutoAccrualFeedbackProcessingErrorDetails? ErrorDetails { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public MileDetails? MileDetails { get; set; }
    }

}
