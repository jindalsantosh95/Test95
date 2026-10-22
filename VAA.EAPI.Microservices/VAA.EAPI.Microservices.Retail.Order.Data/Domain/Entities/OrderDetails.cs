using System.Collections.Generic;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities
{
    public class OrderDetails : CosmosDocument
    {
        public string OrderRef { get; set; }
        public string SchemaVersion { get; set; }
        public string DocType { get; set; }
        public string OrderCreationDate { get; set; }
        public string OrderCreationTime { get; set; }
        public string OrderStatus { get; set; }
        public string Channel { get; set; }
        public List<FulfillmentDetails> FulfilmentId { get; set; }
        public List<Payment> Payments { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdatedDateTime { get; set; }
    }
}
