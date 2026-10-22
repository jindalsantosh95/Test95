namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class ClassAvailability
    {
        public string Compartment { get; set; }
        public string Class { get; set; }
        public int? AuthorisedAvailability { get; set; }
        public bool ProhibitSalesIndicator { get; set; }
        public int Hierachy { get; set; }
        public bool WaitlistOpen { get; set; }
    }
}
