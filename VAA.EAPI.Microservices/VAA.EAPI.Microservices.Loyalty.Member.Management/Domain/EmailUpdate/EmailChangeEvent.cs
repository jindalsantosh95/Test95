using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.EmailUpdate
{
    public class EmailChangeEvent
    {
        public string CustomerId { get; set; }
        public string MemberId { get; set; }
        public string OldEmailAddress { get; set; }
        public string NewEmailAddress { get; set; }
        public string InitiatedBy { get; set; }
    }
}
