using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual
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
