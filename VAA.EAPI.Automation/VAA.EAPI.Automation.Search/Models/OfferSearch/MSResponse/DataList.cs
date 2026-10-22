namespace VAA.EAPI.Automation.Search.Models.OfferSearch.MSResponse
{
    public class DataList
    {
        public List<OriginDestinationAssociation> originDestinationAssociations { get; set; }
        public List<CustomerDetail> customerDetails { get; set; }
        public List<FlightSegment> flightSegments { get; set; }
        public List<Journey> journey { get; set; }
    }
}
