namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass
{
    public class BoardingPassRequest
    {
        public string TravelPartyId { get; set; }
        public string PNRLocator { get; set; }
        public string OriginCode { get; set; }
    }
}
