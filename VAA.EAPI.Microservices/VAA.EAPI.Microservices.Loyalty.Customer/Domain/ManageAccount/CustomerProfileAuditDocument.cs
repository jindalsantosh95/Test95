using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount
{
    public class CustomerProfileAuditDocument
    {
        public string SourceIdentifier { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Title { get; set; }
        public DateTime? DoB { get; set; }
        public string Status { get; set; }
        public string NameChangeReason { get; set; }
        public string AuthorisedUser { get; set; }
        public string DowngradeProhibitFlag { get; set; }
        public string Gender { get; set; }
        public string CustomerProfileUpdatedBy { get; set; }
        public DateTime? CustomerProfileUpdatedOn { get; set; }
        public string CustomerId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set;}
    }
}
