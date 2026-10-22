using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain.Enum;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public class CustomerOrderDetailsDTO : CosmosDocument
    {
        public string DocType { get; set; }
        public string OrderID { get; set; }
        public string CustomerID { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string CreatedOn { get; set; }
        public string LastUpdatedOn { get; set; }
        public int ttl { get; set; }

        public static explicit operator CustomerOrderDetailsDTO(CustomerOrderDetails customerOrder)
        {
            if (customerOrder == null) throw new ArgumentNullException(nameof(customerOrder));

            return new()
            {
                Id = customerOrder.Id ?? Guid.NewGuid().ToString(),
                OrderID = customerOrder.OrderID ?? string.Empty,
                CustomerID = customerOrder.CustomerID ?? string.Empty,
                CustomerPhone = customerOrder.CustomerPhone ?? string.Empty,
                CustomerEmail = !string.IsNullOrEmpty(customerOrder.CustomerEmail) ? customerOrder.CustomerEmail.ToUpper() : string.Empty,
                CreatedOn = !string.IsNullOrEmpty(customerOrder.CreatedOn) ? DateTime.Parse(customerOrder.CreatedOn).ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedOn = DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                PartitionKey = DocumentType.CustomerOrderAssociation.ToString(),
                DocType = DocumentType.CustomerOrderAssociation.ToString(),
                ttl = customerOrder.TimeToLive
            };
        }
    }

}
