using System;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models
{
    public class PartnerProfile
    {
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerType { get; set; }
        public string PartnerSubtype { get; set; }
        public bool Active { get; set; }
        public string CodeShareFlag { get; set; }
        public string ExceptionBookingClassFlag { get; set; }
        public string AutomatedRetroPartner { get; set; }
        public long? MaxTransactionLimit { get; set; }
        public DateTime? PartnerStartDate { get; set; }
        public DateTime? PartnerEndDate { get; set; }
    }
}
