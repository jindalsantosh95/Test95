namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class Booking
    {
        public LeadCustomer LeadPassenger { get; set; }
        public List<Customer> Passengers { get; set; }
        public Holiday Holiday { get; set; }
        public Payment? Payment { get; set; }
        public List<Destination> Destinations { get; set; }
        public List<string> Documents { get; set; }
        public ContactCentre? DomesticContactCentre { get; set; }
        public string? BookingDate { get; set; }
        public List<Experience> Experiences { get; set; }
    }
}

