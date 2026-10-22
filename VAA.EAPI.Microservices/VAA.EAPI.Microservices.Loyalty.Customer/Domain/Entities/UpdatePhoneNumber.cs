using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities
{
    public class UpdatePhoneNumber
    {
        public string CustomerId { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; }
        public string FcNumber { get; set; }
    }
}
