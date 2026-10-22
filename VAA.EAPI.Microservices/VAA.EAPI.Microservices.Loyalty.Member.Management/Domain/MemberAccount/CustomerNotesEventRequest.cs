namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.MemberAccount
{
    public class CustomerNotesEventRequest
    {
        public ContactNote ContactNote { get; set; }
        public string CustomerId { get; set; }
    }
    public class ContactNote
    {
        public string Group { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public string Id { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}

