using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.IFGAgentFopLookUp
{
    public class IFGAgentFopLookUpResponse
    {
        public bool HasTicketingAuthority { get; set; }
        public string PciDetails { get; set; }
        public bool Active { get; set; }
        public bool PciDss { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string CountryCode { get; set; }
        public string TrackID { get; set; }
        public string BspCode { get; set; }
        public bool BspCashAllowed { get; set; }        
        public bool CreditCardAllowed { get; set; }
        public bool EasyPayAllowed { get; set; }
        public string Status { get; set; }
        public string HttpStatus { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> ClientFormsOfPayment  { get; set; }
        
    }
}
