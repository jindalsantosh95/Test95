using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities
{
    public class MemberEnrolmentGraphRequest
    {
        public bool AccountEnabled { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public List<string> OtherMails { get; set; }
        public PasswordProfile PasswordProfile { get; set; }
        public string SecurityQuestion1 { get; set; }
        public string SecurityAnswer1 { get; set; }
        public string SecurityQuestion2 { get; set; }
        public string SecurityAnswer2 { get; set; }
        public string MemberStatus { get; set; }
        public string MemberType { get; set; }
        public string CustomerId { get; set; }
        public string UserPrincipalName { get; set; }
        public string PasswordPolicies { get; set; } = Constants.Application.DisablePasswordExpiration;
        public List<IdentityType> Identities { get; set; }
        public string MailNickname { get; set; }
        public string Mail { get; set; }
    }



}
