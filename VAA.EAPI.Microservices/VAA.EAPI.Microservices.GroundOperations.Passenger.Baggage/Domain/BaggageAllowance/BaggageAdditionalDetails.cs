namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.BaggageAllowance
{
    public class BaggageAdditionalDetails
    {
        public AllAboutBagsResponse AllAboutBags { get; set; }
        public BaggageAdvisory InfantBaggageAdvisory { get; set; }
        public BaggageAdvisory AlreadyPurchasedBaggageAdvisory { get; set; }
        public BaggageAdvisory UnsupportedCurrencyBaggageAdvisory { get; set; }
        public IEnumerable<string> DisablePaidBaggageCurrencies { get; set; }
    }
}
