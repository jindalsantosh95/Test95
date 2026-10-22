using System;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Base
{
    public abstract class CustomerTransaction
    {
        public string Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string CustomerId { get; set; }
    }
}
