using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount
{
    public class ManageAccountAzureB2CUpdateRequest
    {
        public string MemberNumber { get; set; }
        public string CurrentEmailId { get; set; }
        public string NewEmailId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
