using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference
{
    public class PartnerDataMessage : CloudEvent
    {
        public new PartnerDataPayload Data
        {
            get => base.Data as PartnerDataPayload;
            set => base.Data = value;
        }
    }
}
