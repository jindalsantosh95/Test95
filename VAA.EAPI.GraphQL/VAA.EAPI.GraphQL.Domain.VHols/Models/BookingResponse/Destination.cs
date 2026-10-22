namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class Destination
    {
        public string? Name { get; set; }
        public int? LocationId { get; set; }
        public string? LocationCode { get; set; }
        public string? LocationType { get; set; }
        public ContactCentre? ContactCentre { get; set; }
        public Country? Country { get; set; }
        public List<string> Images { get; set; }
        public List<string> HeaderImages { get; set; }
        public List<string> FooterImages { get; set; }
    }
}

