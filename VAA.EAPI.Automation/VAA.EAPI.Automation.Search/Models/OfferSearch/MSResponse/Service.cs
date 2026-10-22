namespace VAA.EAPI.Automation.Search.Models.OfferSearch.MSResponse
{
    public class Service
    {
        public List<string> serviceBeneficiaries { get; set; }
        public List<string> journeyRefIds { get; set; }
        public List<string> flightSegmentsRefIds { get; set; }
        public FareDetails fareDetails { get; set; }
    }
}
