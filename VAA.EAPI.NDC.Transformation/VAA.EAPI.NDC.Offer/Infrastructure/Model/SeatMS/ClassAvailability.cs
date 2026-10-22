namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS
{
    public class ClassAvailability
    {
        public string Compartment { get; set; }
        public string _Class { get; set; }
        public int? AuthorisedAvailability { get; set; }
        public bool? ProhibitSalesIndicator { get; set; }
        public int? Hierachy { get; set; }
        public bool? WaitlistOpen { get; set; }
    }
}
