namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.AddressUpdate
{
    public class AddressUpdateNotificationEventData
    {
        public string CustomerId { get; set; }
        public string InitiatedBy { get; set; }
        public string MemberId { get; set; }
    }
}
