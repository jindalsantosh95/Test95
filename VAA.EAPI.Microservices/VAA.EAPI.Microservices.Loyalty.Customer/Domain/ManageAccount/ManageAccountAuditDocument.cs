using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount
{
    public class ManageAccountAuditDocument
    {
        public CustomerProfileAuditDocument CustomerProfileAuditDocument { get; set; }
    }
}
