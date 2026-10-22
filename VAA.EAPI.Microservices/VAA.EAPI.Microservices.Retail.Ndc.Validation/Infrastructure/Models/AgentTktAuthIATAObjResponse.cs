using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models
{
    public class AgentTktAuthIATAObjResponse
    {
        public int OrgId { get; set; }
        public int RowId { get; set; }
        public string Status { get; set; }
        public string CompanyName { get; set; }
        public string AgencyId { get; set; }
        public string BusinessType { get; set; }
        public string AgencyIdType { get; set; }
        public string CorporateId { get; set; }
        public bool CorporateId_mod { get; set; }
        public string CorporateAccountCode { get; set; }
        public bool CorporateAccountCode_mod { get; set; }
        public string Environment { get; set; }
        public string KeyStatus { get; set; }
        public string KeyType { get; set; }
        public int KeyId { get; set; }
        public int ParentId { get; set; }
        public string RequestOrgName { get; set; }
        public string RequestOrgStatus { get; set; }
        public string RequestAgencyId { get; set; }
        public string RequestAgencyIdType { get; set; }
        public string AccountCode { get; set; }
        public bool AccountCode_mod { get; set; }
        public string SubscriberKey { get; set; }
        public DateTime? ExpiryDateKey { get; set; }
        public DateTime? ExpiryDateKey2 { get; set; }
        public string SubscriberKey2 { get; set; }
        public string CountryCode { get; set; }
        public bool CountryCode_mod { get; set; }
        public bool? BspActiveStatusInd { get; set; }
        public bool? BspActiveStatusInd_mod { get; set; }
        public bool? PciDssComplianceInd { get; set; }
        public bool? PciDssComplianceInd_mod { get; set; }
        public bool? CardFopAllowedInd { get; set; }
        public bool? CardFopAllowedInd_mod { get; set; }
        public bool? CashFopAllowedInd { get; set; }
        public bool? CashFopAllowedInd_mod { get; set; }
        public bool? EasyPayFopAllowedInd { get; set; }
        public bool? EasyPayFopAllowedInd_mod { get; set; }
        public bool? AirlineTicketingAuthorityInd { get; set; }
        public bool? AirlineTicketingAuthorityInd_mod { get; set; }
        public DateTime? LastUpdate { get; set; }
        public DateTime? LastUpdateIATA { get; set; }
    }
}
