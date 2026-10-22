using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class PartnerDetails : TableEntity
    {
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string PartnerType { get; set; }
        public string PartnerSubType { get; set; }
        public bool SkyTeamPartnerFlag { get; set; }
    }
}
