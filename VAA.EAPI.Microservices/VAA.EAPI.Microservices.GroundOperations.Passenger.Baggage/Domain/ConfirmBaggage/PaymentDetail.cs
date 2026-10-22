namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.ConfirmBaggage
{
    public class PaymentDetail
    {
        public string PaymentAmount { get; set; }
        public string DecimalPrecisionCount { get; set; }
        public string CurrencyCode { get; set; }
        public bool PayWithMilesToggle { get; set; }
    }
}
