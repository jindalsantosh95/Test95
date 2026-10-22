namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.BaggageAllowance
{
    public class BaggagePrice
    {
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public decimal AdditionalBagPrice { get; set; }
        public int DecimalPrecisionCount { get; set; }
    }
}
