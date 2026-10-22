using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual
{
    public class AutoAccrualFeedbackProcessingMessage : CloudEvent
    {
        public new AutoAccrualFeedbackProcessingPayload? Data
        {
            get => base.Data as AutoAccrualFeedbackProcessingPayload;
            set => base.Data = value;
        }
    }
}
