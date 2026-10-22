using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual
{
    public class AutoAccrualMessage : CloudEvent
    {
        public new AutoAccrualPayload? Data
        {
            get => base.Data as AutoAccrualPayload;
            set => base.Data = value;
        }
    }
}
