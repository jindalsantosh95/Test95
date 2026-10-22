using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;

namespace VAA.EAPI.Microservices.CRM.Email.Domain
{
    public class AdobeMemberInviteRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MembershipNumber { get; set; }
        public string FirstNameOfHouseholdLead { get; set; }
        public string FirstNameOfHouseholdMember { get; set; }
    }
}
