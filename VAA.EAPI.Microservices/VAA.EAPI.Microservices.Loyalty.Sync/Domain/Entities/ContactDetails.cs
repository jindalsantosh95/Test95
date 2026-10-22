namespace VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities
{
    public class ContactDetails
    {
        public EmailDetail[] EmailDetails { get; set; }
        public PhoneDetail[] PhoneDetails { get; set; }
    }
}