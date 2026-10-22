using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference
{
    public class ProductDataMessage : CloudEvent
    {
        public new ProductDataPayload Data
        {
            get => base.Data as ProductDataPayload;
            set => base.Data = value;
        }
    }
}
