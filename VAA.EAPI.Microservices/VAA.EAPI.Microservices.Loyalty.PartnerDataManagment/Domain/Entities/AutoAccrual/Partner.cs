namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual
{
    public class Partner
    {
        public string PartnerId { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string PartnerType { get; set; }
        public string PartnerSubType { get; set; }
        public bool SkyTeamPartnerFlag { get; set; }
    }
}
