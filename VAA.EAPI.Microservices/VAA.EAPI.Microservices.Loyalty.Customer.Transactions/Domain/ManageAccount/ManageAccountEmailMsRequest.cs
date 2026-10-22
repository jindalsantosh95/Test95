using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount
{
    public class ManageAccountEmailMsRequest
    {
        public string To { get; set; }
        public string Template { get; set; }
        public string FirstName { get; set; }
        public string TierCode { get; set; }
    }
}
