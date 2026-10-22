namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaEncryptedBookingDetails
{
    public class EncryptionResponse
    {
        public string RecordLocator { get; set; }
        public string BidUpgradeToken { get; set; }
        public string ChaufferToken { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public IEnumerable<Trip> Trips { get; set; }
    }
}