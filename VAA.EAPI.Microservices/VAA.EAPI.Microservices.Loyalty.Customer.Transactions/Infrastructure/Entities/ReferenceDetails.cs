using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class ReferenceDetails : TableEntity
    {
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string ProductDescription { get; set; }
        public string ProductCategory { get; set; }
        public string CategoryCode { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string PartnerType { get; set; }
        public string PartnerSubType { get; set; }
    }
}
