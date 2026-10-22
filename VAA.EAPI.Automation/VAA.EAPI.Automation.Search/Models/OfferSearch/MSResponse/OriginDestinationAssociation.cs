namespace VAA.EAPI.Automation.Search.Models.OfferSearch.MSResponse
{
    public class OriginDestinationAssociation
    {
        public string odLabel { get; set; }
        public OriginDestination originDestination { get; set; }
        public List<string> journeyRefIds { get; set; }
    }
}
