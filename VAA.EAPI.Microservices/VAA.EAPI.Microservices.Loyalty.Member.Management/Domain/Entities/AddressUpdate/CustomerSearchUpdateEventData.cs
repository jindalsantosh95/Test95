namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.AddressUpdate
{
    public class CustomerSearchUpdateEventData
    {
        public string LoyaltyMemberNumber { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
    }
}
