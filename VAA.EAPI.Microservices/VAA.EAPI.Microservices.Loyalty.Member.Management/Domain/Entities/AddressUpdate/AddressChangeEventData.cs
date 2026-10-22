namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.AddressUpdate
{
    public class AddressChangeEventData
    {
        public string CustomerId { get; set; }
        public string InitiatedBy { get; set; }
        public AddressDetail AddressDetail { get; set; }
    }
}
