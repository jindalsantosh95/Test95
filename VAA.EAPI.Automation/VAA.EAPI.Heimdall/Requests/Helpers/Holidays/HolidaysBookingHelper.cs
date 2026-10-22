namespace VAA.EAPI.Automation.Heimdall.Requests.Helpers.Holidays
{
    public class BookingRequest
    {
        public string bookingType { get; set; }
        public string location { get; set; }
        public string hotelName { get; set; }
        public string departureDate { get; set; }
        public string duration { get; set; }
        public string gateway { get; set; }
        public List<string> outIATACodes { get; set; } = new List<string>();
        public List<string> inIATACodes { get; set; } = new List<string>();
        public List<string> outCabins { get; set; } = new List<string>();
        public List<string> inCabins { get; set; } = new List<string>();
        public List<string> outFlightNums { get; set; } = new List<string>();
        public List<string> inFlightNums { get; set; } = new List<string>();
        public List<PartyComposition> partyCompositions { get; set; } = new List<PartyComposition>();
        public bool direct { get; set; }
    }
}