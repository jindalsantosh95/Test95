namespace VAA.EAPI.Automation.Heimdall.Requests.Helpers.Holidays
{
    public class PartyComposition
    {
        public int Adults { get; set; }
        public List<int> ChildAges { get; set; } = new List<int>();
        public int Infants { get; set; }
    }

    public class SearchRequest
    {
        public string DepartureDate { get; set; }
        public string Gateway { get; set; }
        public string Duration { get; set; }
        public string BookingType { get; set; }
        public string Location { get; set; }
        public List<PartyComposition> PartyCompositions { get; set; } = new List<PartyComposition>();
        public bool Direct { get; set; }
    }
}