namespace VAA.EAPI.Automation.OCI.Models.GraphQL.SaveBaggage
{
    public class SelectedBaggageOffer
    {
        public string passengerNiN { get; set; }
        public string baggageOfferId { get; set; }
        public int baggageCount { get; set; }
        public decimal totalPrice { get; set; }
        public string currencyCode { get; set; }
        public int decimalPrecisionCount { get; set; }
    }
}