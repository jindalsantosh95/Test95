namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities
{
    public class EmailTriggeringEventData
    {
        public string Status { get; set; }
        public string Source { get; set; }
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }

    }
}
