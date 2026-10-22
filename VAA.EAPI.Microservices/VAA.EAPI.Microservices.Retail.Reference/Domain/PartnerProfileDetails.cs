using System;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class PartnerProfileDetails
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
        public string PartnerTerritoryCode { get; set; }
        public string PartnerSubtype { get; set; }

        public static explicit operator PartnerProfileDetails(PartnerProfile partner) => new()
        {
            Name = partner.Name,
            PartnerCode = partner?.PartnerCode,
            RowId = partner?.RowId,
            PartnerType = partner?.PartnerType,
            PartnerSubtype = partner?.PartnerSubtype
        };
    }
}
