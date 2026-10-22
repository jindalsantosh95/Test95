using System;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models
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
        public string PartnerSubtype { get; set; }
    }
}
