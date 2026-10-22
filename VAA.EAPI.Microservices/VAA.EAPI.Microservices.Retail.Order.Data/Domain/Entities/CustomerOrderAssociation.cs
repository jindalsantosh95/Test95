
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities
{
    public class CustomerOrderAssociation: CosmosDocument
    {
        public string DocType {  get; set; }
        public string OrderID {  get; set; }
        public string CustomerFCNumber { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string CreatedOn { get; set; }
        public string UpdatedOn { get; set; }
        public string LastUpdatedOn { get; set; }

    }
}
