using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberSearchViaPhone
{
    public class MemberInfo
    {
        public string EnvelopeSalutation { get; set; }
        public string MemberNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string CurrentTier { get; set; }
        public string EmailAddress { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
        public string PhoneType { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneCountryCode { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public string MemberPointBalance { get; set; }
    }
}
