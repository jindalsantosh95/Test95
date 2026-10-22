using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.AddressUpdate
{
    public class CustomerUpdateEventData
    {
        public string CustomerId { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public Address[] AddressDetails { get; set; }
    }
}
