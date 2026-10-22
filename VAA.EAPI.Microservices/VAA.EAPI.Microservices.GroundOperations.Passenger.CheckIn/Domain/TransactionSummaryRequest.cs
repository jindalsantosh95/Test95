namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class TransactionSummaryRequest
    {
        public string TravelPartyId { get; set; } = string.Empty;
        public string RecordLocator { get; set; } = string.Empty;
        public string OriginAirportCode { get; set; } = string.Empty;
        public string TransactionError { get; set; } = string.Empty;
    }
}
