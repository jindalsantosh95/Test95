namespace VAA.EAPI.Automation.OCI.Models.Microservices.SaveBaggage
{
    public class BaggagePricing
    {
        public string passengerNiN { get; set; }
        public string baggageOfferId { get; set; }
        public int baggageCount { get; set; }
        public double totalPrice { get; set; }
    }
}