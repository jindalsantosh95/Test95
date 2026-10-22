namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.SaveBags
{
    public class BaggagePricing
    {
        public string PassengerNiN { get; set; }
        public string BaggageOfferId { get; set; }
        public decimal BaggageCount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
