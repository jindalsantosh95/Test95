using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual
{
    public class AutoAccrualProcessingPayload
    {
        public Guid CorrelationId { get; set; }
        public string MessageId { get; set; } = string.Empty;
    }

    public class AutoAccrualProcessingMessage : CloudEvent
    {
        public new AutoAccrualProcessingPayload? Data
        {
            get => base.Data as AutoAccrualProcessingPayload;
            set => base.Data = value;
        }
    }

    public class AutoAccrualLoyaltyManagementRequest
    {
        public Guid CorrelationId { get; set; }
        public string MessageId { get; set; } = string.Empty;
        public DistanceBasedAccrualPayload AccrualRequest { get; set; } = new();
    }
}
