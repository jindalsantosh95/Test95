using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval
{
    public class PartnersResponse
    {
        public List<PartnerProfile> Partners { get; set; }
    }

    public class PartnerProfile
    {
        public string PartnerId { get; set; }
        public string Name { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerType { get; set; }
        public string PartnerSubtype { get; set; }
        public bool Active { get; set; }
        public string CodeShareFlag { get; set; }
        public string ExceptionBookingClassFlag { get; set; }
        public string AutomatedRetroPartner { get; set; }
        public long? MaxTxnLimitForPoints { get; set; }
        public DateTime? PartnerStartDate { get; set; }
        public DateTime? PartnerEndDate { get; set; }

    }
}
