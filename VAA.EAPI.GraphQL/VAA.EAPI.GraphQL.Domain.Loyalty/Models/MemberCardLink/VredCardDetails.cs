using System;
using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberCardLink
{
    public class VredCardDetails
    {
        public string PartnerUserId { get; set; }
        public string AccountStatus { get; set; }
        public DateTime? CreateUpdateTimestamp { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? Dob { get; set; }
        public string Channel { get; set; }
        public string RedUserId { get; set; }
        public MemberBalance Balance { get; set; }
        public IEnumerable<VredCardLink> CardLinks { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
