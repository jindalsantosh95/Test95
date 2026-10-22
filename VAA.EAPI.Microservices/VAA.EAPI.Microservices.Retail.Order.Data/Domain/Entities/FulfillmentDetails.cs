using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities
{
    public class FulfillmentDetails
    {
        public OrderFulfillmentType IdType { get; set; }
        public string FulfillmentId { get; set; }
    }
}
