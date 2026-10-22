namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.BoardingPass
{
    public class BoardingPassRequest
    {
        public string TravelPartyId { get; set; }
        public string PnrLocator { get; set; }
        public string OriginCode { get; set; }

        public static explicit operator DeltaSsci.InputObject(BoardingPassRequest source)
        {
            return new DeltaSsci.InputObject()
            {
                RecordLocator = source.PnrLocator,
                OriginCode = source.OriginCode
            };
        }
    }
}
