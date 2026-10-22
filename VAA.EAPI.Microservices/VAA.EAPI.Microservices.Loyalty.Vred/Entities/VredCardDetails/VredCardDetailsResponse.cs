using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredCardDetails
{
    public class VredCardDetailsResponse
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
        public Balance Balance { get; set; }
        public IEnumerable<VredCardLink> CardLinks { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
