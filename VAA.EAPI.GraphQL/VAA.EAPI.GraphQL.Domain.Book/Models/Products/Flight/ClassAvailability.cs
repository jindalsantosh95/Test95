namespace VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight
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
