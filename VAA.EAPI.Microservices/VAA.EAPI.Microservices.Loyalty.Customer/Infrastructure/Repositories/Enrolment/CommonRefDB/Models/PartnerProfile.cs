using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Models
{
    public partial class PartnerProfile
    {
        public string RowId { get; set; }
        public string SourceIdentifier { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string AccountTypeCode { get; set; }
        public string CurrencyCode { get; set; }
        public string Description { get; set; }
        public string AutomatedRetroPartner { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerType { get; set; }
        public string Name { get; set; }
        public string PartnerCategory { get; set; }
        public string PartnerTerritoryCode { get; set; }
    }
}
