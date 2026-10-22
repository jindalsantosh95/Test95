namespace VAA.EAPI.Automation.Core.TestDataConfig
{
    public class Order
    {
        public string PNR { get; set; }
        public string Airline { get; set; }
        public string FlightNumber { get; set; }
        public string DepartureDate { get; set; }
        public string PassengerParty { get; set; }
        public string PassengersCount { get; set; }
        public string FlightPassengerSeatMap { get; set; }
        public string LeadPassengerFirstName { get; set; }
        public string LeadPassengerLastName { get; set; }
        public string EmailId { get; set; }
        public string EmailFromBasket { get; set; }
        public string PRI { get; set; }
        public string ChildPRI { get; set; }
        public string HppUrl { get; set; }
        public bool? IsDirectFlight { get; set; }
        public bool? IsCodeShare { get; set; }
        public string OutBoundCabinClass { get; set; }
        public string InBoundCabinClass { get; set; }
        public string PreBookingFCPoints { get; set; }
        public string FcPointsPostBooking { get; set; }
        public string FlyingClubNumber { get; set; }
        public string FqtTierCode { get; set; }
        public string FqtProgramme { get; set; }
        public string FlightPoints { get; set; }
        public bool IsAward { get; set; }
        public string OutboundSectorInformation {  get; set; }
        public string InboundSectorInformation { get; set; }
    }
}