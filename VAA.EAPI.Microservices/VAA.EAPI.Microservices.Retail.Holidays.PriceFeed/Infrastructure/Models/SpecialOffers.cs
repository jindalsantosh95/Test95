namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Models
{
    public class SpecialOffers
    {
        public int Duration { get; set; }
        public int AdjustedDuration { get; set; }
        public double MinGroupPrice { get; set; }
        public string MinCheckInDate { get; set; }
        public string MaxCheckInDate { get; set; }
        public string MinAdjustedCheckInDate { get; set; }
        public string MaxAdjustedCheckInDate { get; set; }
        public string MinBookDateFrom { get; set; }
        public string MaxBookDateTo { get; set; }
        public double OnlineDiscount { get; set; }
        public double Saving { get; set; }
        public int SavingPercentage { get; set; }
        public string DepartureAirport { get; set; }
        public string DepartureAirportCode { get; set; }
        public double BrochurePricePerPerson { get; set; }
        public bool ExtraNight { get; set; }
        public string BoardBasisCode { get; set; }
        public string BoardBasisName { get; set; }
        public string Type { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public int Infants { get; set; }
        public string OriginOfferId { get; set; }
    }
}
