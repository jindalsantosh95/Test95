namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public class PsgBaggageInfo
    {
        public string PassengerNiN { get; set; }
        public string BaggageOfferId { get; set; }
        public int? BaggageCount { get; set; }
        public decimal? TotalPrice { get; set; }
        public string CurrencyCode { get; set; }
        public int? DecimalPrecisionCount { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int NumOfFreeBags { get; set; }
        public int NumOfPaidBags { get; set; }
    }
}
