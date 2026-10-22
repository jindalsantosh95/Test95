using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain
{
    public class CRMTriggerRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string TierCode { get; set; }
        public string B2CPasswordResetLink { get; set; }
        public string VACEnrolEmailTemplate { get; set; }
    }
}
