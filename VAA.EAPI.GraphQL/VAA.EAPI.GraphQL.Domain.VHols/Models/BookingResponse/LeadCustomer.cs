namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class LeadCustomer
    {
        public string? Title { get; set; }
        public string? Forename { get; set; }
        public string? Surname { get; set; }
        public string? DateOfBirth { get; set; }
        public string? CustomerId { get; set; }
        public ContactDetails? ContactDetails { get; set; }
        public List<SpecialRequest> SpecialRequests { get; set; }
    }
}

