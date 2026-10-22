namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass
{
    public class BoardingPassServiceRequest
    {
        public string TravelPartyId { get; set; }
        public string PnrLocator { get; set; }

        public static explicit operator BoardingPassServiceRequest((string travelPartyId, string pnrLocator) source)
        {
            return new BoardingPassServiceRequest()
            {
                TravelPartyId = source.travelPartyId,
                PnrLocator = source.pnrLocator
            };
        }
    }
}
