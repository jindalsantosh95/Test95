namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile
{
    public class FlightProfile
    {

        public bool? FutureFlightsFlag { get; set; }
        public bool? RecentFlightsFlag { get; set; }
        public bool? AnnualFlightsFlag { get; set; }
        public bool? HistoricalFlightsFlag { get; set; }
    }
}
