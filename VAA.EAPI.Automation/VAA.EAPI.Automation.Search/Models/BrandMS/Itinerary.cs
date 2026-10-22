namespace VAA.EAPI.Automation.Search.Models.BrandMS
{
    public class Itinerary
    {
        public string source { get; set; }
        public List<Segment> segments { get; set; }
        public List<string> passengerTypeCodes { get; set; }
    }
}
