using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount
{
    public class EmailTriggerRequestManageAccount
    {
        public string To { get; set; }
        public string Template { get; set; }
    }
}
