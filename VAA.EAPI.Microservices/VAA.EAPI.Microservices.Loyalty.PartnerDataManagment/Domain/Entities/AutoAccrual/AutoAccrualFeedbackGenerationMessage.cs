using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual
{
    public class AutoAccrualFeedbackGenerationMessage : CloudEvent
    {
        public new AutoAccrualFeedbackGenerationPayload? Data
        {
            get => base.Data as AutoAccrualFeedbackGenerationPayload;
            set => base.Data = value;
        }
    }
}
