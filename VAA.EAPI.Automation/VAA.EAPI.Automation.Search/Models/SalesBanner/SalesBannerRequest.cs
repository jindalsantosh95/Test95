namespace VAA.EAPI.Automation.Search.Models.SalesBanner
{
    public class SalesBannerRequest
    {
        public string originAirport { get; set; }
        public string destinationAirport { get; set; }
        public string flightDirection { get; set; }
        public string departureDate { get; set; }
        public string returnDate { get; set; }
        public int numAdults { get; set; }
        public int numTeens { get; set; }
        public int numChildren { get; set; }
        public int numInfants { get; set; }
        public string destinationURL { get; set; }
    }
}