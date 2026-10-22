using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;

namespace VAA.EAPI.Microservices.CRM.Email.Domain
{
    public class VACEnrolEmailRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TierCode { get; set; } = "VAC";
        public string VACEnrolEmailTemplate { get; set; }
        public string EmailAddress { get; set; }
        public string B2CPasswordResetLink { get; set; }

    }
}
