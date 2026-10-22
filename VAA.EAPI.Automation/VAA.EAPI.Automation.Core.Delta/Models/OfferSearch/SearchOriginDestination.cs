namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch
{
    public class SearchOriginDestination
    {
        public string departureLocalTs { get; set; }
        public List<Destination> destinations { get; set; }
        public List<Origin> origins { get; set; }
    }
}